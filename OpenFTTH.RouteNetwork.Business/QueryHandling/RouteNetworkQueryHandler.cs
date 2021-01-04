﻿using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using OpenFTTH.CQRS;
using OpenFTTH.RouteNetwork.API.Queries;
using OpenFTTH.RouteNetwork.Business.StateHandling;
using System;
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
            return Task.FromResult(
                _routeNodeRepository.GetRouteElements(query)
            );
        }
    }
}


