using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.API.Queries;
using OpenFTTH.RouteNetwork.Service.Business.DomainModel.RouteNetwork;
using System;
using System.Collections.Generic;

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

        public Result<GetRouteNetworkDetailsQueryResult> GetRouteElements(GetRouteNetworkDetailsQuery query)
        {
            var routeNetworkElementFetched = new List<IRouteNetworkElement>();

            foreach (var routeElementId in query.RouteNetworkElementIdsToQuery)
            {
                var routeNetworkElement = _routeNetworkState.GetRouteNetworkElement(routeElementId);

                if (routeNetworkElement == null)
                {
                    return Result.Failure<GetRouteNetworkDetailsQueryResult>($"Cannot find any route network element with id: {routeElementId}");
                }

                routeNetworkElementFetched.Add(routeNetworkElement as IRouteNetworkElement);
            }


            return Result.Success<GetRouteNetworkDetailsQueryResult>(new GetRouteNetworkDetailsQueryResult(MapRouteElementDomainObjectsToQueryObjects(routeNetworkElementFetched)));
        }

        private RouteNetworkElement[] MapRouteElementDomainObjectsToQueryObjects(List<IRouteNetworkElement> routeNetworkElements)
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
