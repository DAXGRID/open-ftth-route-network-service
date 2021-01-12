using OpenFTTH.Events.RouteNetwork.Infos;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFTTH.RouteNetwork.Service.Business.DomainModel.RouteNetwork
{
    public interface IRouteNode : IRouteNetworkElement
    {
        public RouteNodeInfo? RouteNodeInfo { get; }
    }
}
