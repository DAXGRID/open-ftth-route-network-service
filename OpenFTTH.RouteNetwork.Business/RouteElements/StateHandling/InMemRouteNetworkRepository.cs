using FluentResults;
using Microsoft.Extensions.Logging;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.Business.RouteElements.Model;
using System;
using System.Collections.Generic;

namespace OpenFTTH.RouteNetwork.Business.RouteElements.StateHandling
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

        public IRouteNetworkState NetworkState => _routeNetworkState;

        public Result<List<IRouteNetworkElement>> GetRouteElements(RouteNetworkElementIdList routeNetworkElementIds)
        {
            var routeNetworkElementFetched = new List<IRouteNetworkElement>();

            foreach (var routeElementId in routeNetworkElementIds)
            {
                var routeNetworkElement = _routeNetworkState.GetRouteNetworkElement(routeElementId);

                if (routeNetworkElement == null)
                {
                    return Result.Fail<List<IRouteNetworkElement>>($"Cannot find any route network element with id: {routeElementId}");
                }

                routeNetworkElementFetched.Add(routeNetworkElement as IRouteNetworkElement);
            }

            return Result.Ok<List<IRouteNetworkElement>>(routeNetworkElementFetched);
        }

    }
}
