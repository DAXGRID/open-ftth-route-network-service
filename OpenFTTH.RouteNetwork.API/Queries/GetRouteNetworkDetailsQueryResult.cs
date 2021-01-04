using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
using OpenFTTH.RouteNetwork.API.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetwork.API.Queries
{
    public record GetRouteNetworkDetailsQueryResult
    {
        public RouteNetworkElement[] ElementsInfos { get; }

        public GetRouteNetworkDetailsQueryResult(RouteNetworkElement[] routeElementInfos)
        {
            this.ElementsInfos = routeElementInfos;
        }
    }
}
