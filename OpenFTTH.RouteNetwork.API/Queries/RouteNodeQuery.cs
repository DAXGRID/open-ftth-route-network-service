using CSharpFunctionalExtensions;
using MediatR;
using OpenFTTH.CQRS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetwork.API.Queries
{
    /// <summary>
    /// Request used to query a route node
    /// </summary>
    public class RouteNodeQuery : IQuery<Result<RouteNodeQueryResult>>
    {
        public string RequestName => typeof(RouteNodeQuery).Name;

        public Guid RouteNodeId { get; }

        public RouteNodeQuery(Guid routeNodeId)
        {
            RouteNodeId = routeNodeId;
        }
    }
}
