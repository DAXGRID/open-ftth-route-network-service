using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetworkService.Queries
{
    /// <summary>
    /// Request used to query a route segment
    /// </summary>
    public class RouteSegmentQuery : IRequest<RouteSegmentQueryResult>
    {
        public string RequestName => typeof(RouteSegmentQuery).Name;

        public Guid RouteSegmentId { get; }

        public RouteSegmentQuery(Guid routeSegmentId)
        {
            RouteSegmentId = routeSegmentId;
        }
    }
}
