using MediatR;
using OpenFTTH.RouteNetworkService.Queries;
using OpenFTTH.RouteNetworkService.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetworkService.QueryHandlers
{
    public class RouteNodeQueryHandler : IRequestHandler<RouteNodeQuery, RouteNodeQueryResult>
    {
        private readonly IRouteNodeRepository _routeNodeRepository;

        public RouteNodeQueryHandler(IRouteNodeRepository routeNodeRepository)
        {
            _routeNodeRepository = routeNodeRepository;
        }

        Task<RouteNodeQueryResult> IRequestHandler<RouteNodeQuery, RouteNodeQueryResult>.Handle(RouteNodeQuery routeNodeQuery, CancellationToken cancellationToken)
        {
            return Task.FromResult(_routeNodeRepository.Query(routeNodeQuery));
        }
    }
}


