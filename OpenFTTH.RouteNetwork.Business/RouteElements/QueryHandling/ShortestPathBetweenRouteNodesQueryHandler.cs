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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Roy_T.AStar.Graphs;
using Roy_T.AStar.Primitives;
using Roy_T.AStar.Paths;

namespace OpenFTTH.RouteNetwork.Business.RouteElements.QueryHandlers
{
    public class ShortestPathBetweenRouteNodesQueryHandler :
        IQueryHandler<ShortestPathBetweenRouteNodes, Result<ShortestPathBetweenRouteNodesResult>>
    {
        private readonly ILogger<FindNearestRouteNodeQueryHandler> _logger;
        private readonly IEventStore _eventStore;
        private readonly IRouteNetworkRepository _routeNetworkRepository;
        private readonly IRouteNetworkState _routeNetworkState;

        public ShortestPathBetweenRouteNodesQueryHandler(ILoggerFactory loggerFactory, IEventStore eventStore, IRouteNetworkRepository routeNodeRepository, IRouteNetworkState routeNetworkState)
        {
            if (null == loggerFactory)
            {
                throw new ArgumentNullException("loggerFactory is null");
            }

            _logger = loggerFactory.CreateLogger<FindNearestRouteNodeQueryHandler>();

            _eventStore = eventStore;
            _routeNetworkRepository = routeNodeRepository;
            _routeNetworkState = routeNetworkState;
        }

        public Task<Result<ShortestPathBetweenRouteNodesResult>> HandleAsync(ShortestPathBetweenRouteNodes query)
        {
            Stopwatch sw = new();
            sw.Start();

            var sourceNode = _routeNetworkRepository.NetworkState.GetRouteNetworkElement(query.SourceRouteNodeId) as RouteNode;

            if (sourceNode == null)
            {
                return Task.FromResult(
                     Result.Fail<ShortestPathBetweenRouteNodesResult>(new FindNearestRouteNodesError(FindNearestRouteNodesErrorCodes.INVALID_QUERY_ARGUMENT_ERROR_LOOKING_UP_SPECIFIED_ROUTE_NETWORK_ELEMENT_BY_ID, $"Error looking up route network node with id: {query.SourceRouteNodeId}"))
                 );
            }


            return Task.FromResult(
                Result.Ok<ShortestPathBetweenRouteNodesResult>(
                    null
                )
            );
        }

        private int NumberOfShortestPathTracesWithinDistance(IEnumerable<NearestRouteNodeTraceResult> nodeTraceResults, double distance)
        {
            int tracesWithinDistance = 0;

            foreach (var nodeTrace in nodeTraceResults)
            {
                if (nodeTrace.Distance <= distance)
                    tracesWithinDistance++;
            }

            return tracesWithinDistance;
        }

    }

}


