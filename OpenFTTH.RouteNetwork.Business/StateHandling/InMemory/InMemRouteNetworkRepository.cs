using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using OpenFTTH.RouteNetworkService.Business.Model.RouteNetwork;
using OpenFTTH.RouteNetworkService.Queries;
using System;

namespace OpenFTTH.RouteNetwork.Business.StateHandling.InMemory
{
    public class InMemRouteNetworkRepository : IRouteNetworkRepository
    {
        private readonly ILogger<InMemRouteNetworkRepository> _logger;
        private readonly IRouteNetworkState _routeNetworkState;


        public InMemRouteNetworkRepository(ILoggerFactory loggerFactory, IRouteNetworkState routeNetworkState)
        {
            if (null == loggerFactory)
            {
                throw new ArgumentNullException("loggerFactory is null");
            }

            _logger = loggerFactory.CreateLogger<InMemRouteNetworkRepository>();

            _routeNetworkState = routeNetworkState;
        }

        public Result<RouteNodeQueryResult> QueryNode(RouteNodeQuery query)
        {
            var routeNode = _routeNetworkState.GetObject(query.RouteNodeId);

            if (routeNode == null)
            {
                return Result.Failure<RouteNodeQueryResult>($"Cannot find any route node with id: {query.RouteNodeId}");
            }
            else if (routeNode is not RouteNode)
            {
                return Result.Failure<RouteNodeQueryResult>($"Expected a RouteNode, but got a {routeNode.GetType().Name} looking up object by id: {query.RouteNodeId}");
            }
            else
            {
                return Result.Success<RouteNodeQueryResult>(((RouteNode)routeNode).GetQueryResult());
            }
        }
    }
}
