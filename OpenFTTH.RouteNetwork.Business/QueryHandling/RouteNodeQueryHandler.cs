using OpenFTTH.CQRS;
using OpenFTTH.RouteNetwork.Business.StateHandling;
using OpenFTTH.RouteNetworkService.Queries;
using System;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetworkService.QueryHandlers
{
    public class RouteNetworkQueryHandler :
        IQueryHandler<RouteNodeQuery, RouteNodeQueryResult>,
        IQueryHandler<RouteSegmentQuery, RouteSegmentQueryResult>
    {
        private readonly IRouteNodeNetworkRepository _routeNodeRepository;

        public RouteNetworkQueryHandler(IRouteNodeNetworkRepository routeNodeRepository)
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


