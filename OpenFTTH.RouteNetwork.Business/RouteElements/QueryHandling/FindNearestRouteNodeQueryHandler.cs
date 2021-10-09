using DAX.ObjectVersioning.Graph;
using FluentResults;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Newtonsoft.Json.Linq;
using OpenFTTH.CQRS;
using OpenFTTH.Events.RouteNetwork.Infos;
using OpenFTTH.EventSourcing;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.API.Queries;
using OpenFTTH.RouteNetwork.Business.RouteElements.Model;
using OpenFTTH.RouteNetwork.Business.RouteElements.StateHandling;
using QuikGraph;
using QuikGraph.Algorithms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetwork.Business.RouteElements.QueryHandlers
{
    public class FindNearestRouteNodeQueryHandler :
        IQueryHandler<FindNearestRouteNodes, Result<FindNearestRouteNodesResult>>
    {
        private readonly ILogger<FindNearestRouteNodeQueryHandler> _logger;
        private readonly IEventStore _eventStore;
        private readonly IRouteNetworkRepository _routeNodeRepository;
        private readonly IRouteNetworkState _routeNetworkState;

        public FindNearestRouteNodeQueryHandler(ILoggerFactory loggerFactory, IEventStore eventStore, IRouteNetworkRepository routeNodeRepository, IRouteNetworkState routeNetworkState)
        {
            if (null == loggerFactory)
            {
                throw new ArgumentNullException("loggerFactory is null");
            }

            _logger = loggerFactory.CreateLogger<FindNearestRouteNodeQueryHandler>();

            _eventStore = eventStore;
            _routeNodeRepository = routeNodeRepository;
            _routeNetworkState = routeNetworkState;
        }

        public Task<Result<FindNearestRouteNodesResult>> HandleAsync(FindNearestRouteNodes query)
        {
            Stopwatch sw = new();
            sw.Start();

            var getRouteNetworkElementsResult = _routeNodeRepository.GetRouteElements(new RouteNetworkElementIdList() { query.SourceRouteNodeId });

            // Here we return a error result, because we're dealing with invalid route network ids provided by the client
            if (getRouteNetworkElementsResult.IsFailed || getRouteNetworkElementsResult.Value.Count != 1)
                return Task.FromResult(
                    Result.Fail<FindNearestRouteNodesResult>(new FindNearestRouteNodesError(FindNearestRouteNodesErrorCodes.INVALID_QUERY_ARGUMENT_ERROR_LOOKING_UP_SPECIFIED_ROUTE_NETWORK_ELEMENT_BY_ID, $"Error looking up route network node with id: {query.SourceRouteNodeId}")).
                    WithError(getRouteNetworkElementsResult.Errors.First())
                );

            var sourceRouteNode = getRouteNetworkElementsResult.Value.First() as RouteNode;

            if (sourceRouteNode == null)
                return Task.FromResult(
                    Result.Fail<FindNearestRouteNodesResult>(new FindNearestRouteNodesError(FindNearestRouteNodesErrorCodes.INVALID_QUERY_ARGUMENT_ERROR_LOOKING_UP_SPECIFIED_ROUTE_NETWORK_ELEMENT_BY_ID, $"Error looking up route network node. Got { getRouteNetworkElementsResult.Value.First().GetType().Name} querying element with id: {query.SourceRouteNodeId}")).
                    WithError(getRouteNetworkElementsResult.Errors.First())
                );

            var sourceRouteNodePoint = GetPoint(sourceRouteNode.Coordinates);

            long version = _routeNetworkState.GetLatestCommitedVersion();
            var stopHash = query.NodeKindStops.ToHashSet();
            var interestHash = query.NodeKindOfInterests.ToHashSet();

            // Fetch objects from route network graph to query on
            var routeNetworkSubset = sourceRouteNode.UndirectionalDFS<RouteNode, RouteSegment>(
                version: version, 
                nodeCriteria: n => (n.RouteNodeInfo == null || n.RouteNodeInfo.Kind == null || !stopHash.Contains(n.RouteNodeInfo.Kind.Value)) && GetPoint(n.Coordinates).Distance(sourceRouteNodePoint) < query.SearchRadiusMeters, 
                includeElementsWhereCriteriaIsFalse: true
            );

            // Find nodes to check/trace shortest path
            var graphForTracing = GetGraphForTracing(version, routeNetworkSubset);
            var nodeCandidatesToTrace = GetNodesToCheckOrderedByDistanceToSourceNode(sourceRouteNode, interestHash, routeNetworkSubset);

            List<RouteNetworkTrace> nodeTraceResults = new();

            int nShortestPathTraces = 0;

            foreach (var nodeToTrace in nodeCandidatesToTrace)
            {
                var shortestPathTrace = ShortestPath(nodeToTrace.Item1, sourceRouteNode.Id, graphForTracing);
                nodeTraceResults.Add(shortestPathTrace);
                nShortestPathTraces++;

                if (NumberOfShortestPathTracesWithinDistance(nodeTraceResults, nodeToTrace.Item2) >= query.MaxHits)
                    break;
            }

            var nodeTraceResultOrdered = nodeTraceResults.OrderBy(n => n.Distance).ToList();

            sw.Stop();
            _logger.LogInformation($"{nShortestPathTraces} shortets path trace(s) processed in {sw.ElapsedMilliseconds} milliseconds finding the {query.MaxHits} nearest nodes to source node with id: {sourceRouteNode.Id}");

            List<RouteNetworkTrace> tracesToReturn = new();
            List<IRouteNetworkElement> routeNodeElementsToReturn = new();

            for (int i = 0; i < query.MaxHits && i < nodeTraceResultOrdered.Count; i++)
            {
                // Add trace
                var traceToAdd = nodeTraceResultOrdered[i];

                tracesToReturn.Add(traceToAdd);

                var routeElementToAdd = _routeNodeRepository.NetworkState.GetRouteNetworkElement(traceToAdd.DestNodeId);

                if (routeElementToAdd != null)
                    routeNodeElementsToReturn.Add(routeElementToAdd);
            }

            var result = new FindNearestRouteNodesResult(
                sourceRouteNodeId: sourceRouteNode.Id,
                routeNetworkElements: GetRouteNetworkDetailsQueryHandler.MapRouteElementDomainObjectsToQueryObjects(query.RouteNetworkElementFilter, routeNodeElementsToReturn),
                routeNetworkTraces: tracesToReturn
             );

            return Task.FromResult(
                Result.Ok<FindNearestRouteNodesResult>(
                    result
                )
            );
        }

        private int NumberOfShortestPathTracesWithinDistance(List<RouteNetworkTrace> nodeTraceResults, double distance)
        {
            int tracesWithinDistance = 0;

            foreach (var nodeTrace in nodeTraceResults)
            {
                if (nodeTrace.Distance <= distance)
                    tracesWithinDistance++;
            }

            return tracesWithinDistance;
        }

        private RouteNetworkTrace ShortestPath(RouteNode fromNode, Guid toNodeId, GraphHolder graphHolder)
        {
            Func<Edge<Guid>, double> lineDistances = e => graphHolder.EdgeLengths[e];

            TryFunc<Guid, IEnumerable<Edge<Guid>>> tryGetPath = graphHolder.Graph.ShortestPathsDijkstra(lineDistances, fromNode.Id);

            IEnumerable<Edge<Guid>> path;
            tryGetPath(toNodeId, out path);

            List<Guid> segmentIds = new();
            List<string> segmentGeometries = new();

            double distance = 0;

            if (path != null)
            {
                foreach (var edge in path)
                {
                    segmentIds.Add(graphHolder.EdgeToSegment[edge].Id);
                    segmentGeometries.Add(graphHolder.EdgeToSegment[edge].Coordinates);

                    distance += graphHolder.EdgeLengths[edge];
                }
            }
            return new RouteNetworkTrace(fromNode.Id, fromNode?.NamingInfo?.Name, distance, segmentIds.ToArray(), segmentGeometries.ToArray());
        }

        private static IOrderedEnumerable<(RouteNode, double)> GetNodesToCheckOrderedByDistanceToSourceNode(RouteNode sourceRouteNode, HashSet<RouteNodeKindEnum> interestHash, IEnumerable<IGraphObject> traceResult)
        {
            var sourceRouteNodePoint = GetPoint(sourceRouteNode.Coordinates);

            List<(RouteNode, double)> nodesToCheck = new();

            foreach (var graphObj in traceResult)
            {
                if (graphObj is RouteNode)
                {
                    var routeNode = graphObj as RouteNode;
                    if (routeNode != null && routeNode.RouteNodeInfo != null && routeNode.RouteNodeInfo.Kind != null && interestHash.Contains(routeNode.RouteNodeInfo.Kind.Value))
                    {
                        nodesToCheck.Add((routeNode, GetPoint(routeNode.Coordinates).Distance(sourceRouteNodePoint)));
                    }
                }
            }

            return nodesToCheck.OrderBy(n => n.Item2);
        }

        private static double GetLength(string lineStringJson)
        {
            List<Coordinate> coordinates = new();

            var coordPairs = JArray.Parse(lineStringJson);
            foreach (var coordPair in coordPairs)
            {
                coordinates.Add(new Coordinate(((JArray)coordPair)[0].Value<double>(), ((JArray)coordPair)[1].Value<double>()));
            }

            return new LineString(coordinates.ToArray()).Length;
        }

        private static Point GetPoint(string pointGeojson)
        {
            List<Coordinate> coordinates = new();

            var coordPairs = JArray.Parse(pointGeojson);

            return new Point(((JArray)coordPairs)[0].Value<double>(), ((JArray)coordPairs)[1].Value<double>());
        }


        private static GraphHolder GetGraphForTracing(long version, IEnumerable<IGraphObject> traceResult)
        {
            GraphHolder result = new();

            foreach (var grapObject in traceResult)
            {
                switch (grapObject)
                {
                    case RouteNode node:
                        result.Graph.AddVertex(node.Id);
                        break;
                }
            }

            foreach (var grapObject in traceResult)
            {
                switch (grapObject)
                {
                    case RouteSegment segment:
                        var edge = new Edge<Guid>(segment.InV(version).Id, segment.OutV(version).Id);
                        result.Graph.AddEdge(edge);

                        result.EdgeLengths.Add(edge, GetLength(segment.Coordinates));
                        result.EdgeToSegment.Add(edge, segment);
                        break;
                }
            }


            return result;
        }


    }

    class GraphHolder
    {
        public UndirectedGraph<Guid, Edge<Guid>> Graph { get; set; }
        public Dictionary<object, double> EdgeLengths { get; set; }
        public Dictionary<object, RouteSegment> EdgeToSegment { get; set; }

        public GraphHolder()
        {
            Graph = new UndirectedGraph<Guid, Edge<Guid>>();
            EdgeLengths = new Dictionary<object, double>();
            EdgeToSegment = new Dictionary<object, RouteSegment>();
        }
    }


}


