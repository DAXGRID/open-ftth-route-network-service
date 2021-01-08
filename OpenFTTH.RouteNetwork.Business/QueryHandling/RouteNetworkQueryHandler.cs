using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using OpenFTTH.CQRS;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.API.Queries;
using OpenFTTH.RouteNetwork.Business.StateHandling;
using OpenFTTH.RouteNetwork.Service.Business.DomainModel.RouteNetwork;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetworkService.QueryHandlers
{
    public class RouteNetworkQueryHandler :
        IQueryHandler<GetRouteNetworkDetailsQuery, Result<GetRouteNetworkDetailsQueryResult>>
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

        public Task<Result<GetRouteNetworkDetailsQueryResult>> HandleAsync(GetRouteNetworkDetailsQuery query)
        {
            var getRouteNetworkElementsResult = _routeNodeRepository.GetRouteElements(query.RouteNetworkElementIdsToQuery);

            if (getRouteNetworkElementsResult.IsFailure)
                return Task.FromResult(Result.Failure<GetRouteNetworkDetailsQueryResult>(getRouteNetworkElementsResult.Error));

            var mappedRouteNetworkElements = MapRouteElementDomainObjectsToQueryObjects(getRouteNetworkElementsResult.Value);

            return Task.FromResult(
                Result.Success<GetRouteNetworkDetailsQueryResult>(
                    new GetRouteNetworkDetailsQueryResult(mappedRouteNetworkElements)
                )
            );
        }



        private static RouteNetworkElement[] MapRouteElementDomainObjectsToQueryObjects(List<IRouteNetworkElement> routeNetworkElements)
        {
            var routeNetworkElementDTOs = new List<RouteNetworkElement>();

            foreach (var routeNetworkElement in routeNetworkElements)
            {
                routeNetworkElementDTOs.Add(
                    new RouteNetworkElement(routeNetworkElement.Id, RouteNetworkElementKindEnum.RouteNode)
                );
            }

            return routeNetworkElementDTOs.ToArray();
        }

    }
}


