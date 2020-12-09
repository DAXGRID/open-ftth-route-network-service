using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using OpenFTTH.CQRS;
using OpenFTTH.RouteNetwork.Business.StateHandling;
using OpenFTTH.RouteNetworkService.Queries;
using System;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetworkService.QueryHandlers
{
    public class RouteNetworkQueryHandler :
        IQueryHandler<RouteNodeQuery, Result<RouteNodeQueryResult>>,
        IQueryHandler<RouteSegmentQuery, RouteSegmentQueryResult>
    {
        private readonly ILogger<RouteNetworkQueryHandler> _logger;
        private readonly IRouteNetworkRepository _routeNodeRepository;

        public RouteNetworkQueryHandler(ILoggerFactory loggerFactory, IRouteNetworkRepository routeNodeRepository)
        {
            if (null == loggerFactory)
            {
                throw new ArgumentNullException("loggerFactory is null");
            }

            _logger = loggerFactory.CreateLogger<RouteNetworkQueryHandler>();

            _routeNodeRepository = routeNodeRepository;
        }

        public Task<Result<RouteNodeQueryResult>> HandleAsync(RouteNodeQuery query)
        {
            return Task.FromResult(
                _routeNodeRepository.QueryNode(query)
            );
        }

        public Task<RouteSegmentQueryResult> HandleAsync(RouteSegmentQuery query)
        {
            throw new NotImplementedException();
        }
    }
}


