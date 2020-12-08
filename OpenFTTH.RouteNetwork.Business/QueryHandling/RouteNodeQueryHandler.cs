using MediatR;
using OpenFTTH.CQRS;
using OpenFTTH.RouteNetworkService.Business.StateHandling;
using OpenFTTH.RouteNetworkService.Queries;
using OpenFTTH.RouteNetworkService.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetworkService.QueryHandlers
{
    public class RouteNetworkQueryHandler :
        IQueryHandler<RouteNodeQuery, RouteNodeQueryResult>,
        IQueryHandler<RouteSegmentQuery, RouteSegmentQueryResult>
    {
        private readonly IRouteNodeRepository _routeNodeRepository;

        public RouteNetworkQueryHandler(IRouteNodeRepository routeNodeRepository)
        {
            _routeNodeRepository = routeNodeRepository;
        }

        public Task<RouteNodeQueryResult> HandleAsync(RouteNodeQuery query)
        {
            return Task.FromResult(_routeNodeRepository.Query(query));
        }

        public Task<RouteSegmentQueryResult> HandleAsync(RouteSegmentQuery query)
        {
            throw new NotImplementedException();
        }
    }
}


