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
        private readonly IRouteNetworkRepository _routeNodeRepository;

        public RouteNetworkQueryHandler(IRouteNetworkRepository routeNodeRepository)
        {
            _routeNodeRepository = routeNodeRepository;
        }

        public Task<RouteNodeQueryResult> HandleAsync(RouteNodeQuery query)
        {
            return Task.FromResult(_routeNodeRepository.NodeQuery(query));
        }

        public Task<RouteSegmentQueryResult> HandleAsync(RouteSegmentQuery query)
        {
            throw new NotImplementedException();
        }
    }
}


