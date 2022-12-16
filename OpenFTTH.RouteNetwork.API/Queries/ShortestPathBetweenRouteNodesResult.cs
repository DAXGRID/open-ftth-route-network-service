using System;
using System.Collections.Generic;

namespace OpenFTTH.RouteNetwork.API.Queries
{
    public record ShortestPathBetweenRouteNodesResult
    {
        public List<Guid> RouteNetworkElementIds { get; }

        public ShortestPathBetweenRouteNodesResult(List<Guid> routeNetworkElementIds)
        {
            RouteNetworkElementIds = routeNetworkElementIds;
        }
    }
}
