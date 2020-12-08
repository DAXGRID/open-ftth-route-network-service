using MediatR;
using OpenFTTH.CQRS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetworkService.Queries
{
    /// <summary>
    /// Request used to query a route node
    /// </summary>
    public class RouteNodeQuery : IQuery<RouteNodeQueryResult>
    {
        public string RequestName => typeof(RouteNodeQuery).Name;

        public Guid RouteNodeId { get; }

        public RouteNodeQuery(Guid routeNodeId)
        {
            RouteNodeId = routeNodeId;
        }
    }
}
