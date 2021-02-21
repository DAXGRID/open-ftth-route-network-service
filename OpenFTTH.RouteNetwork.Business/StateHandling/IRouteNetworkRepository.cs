using FluentResults;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.Service.Business.DomainModel.RouteNetwork;
using System.Collections.Generic;

namespace OpenFTTH.RouteNetwork.Business.StateHandling
{
    public interface IRouteNetworkRepository
    {
        Result<List<IRouteNetworkElement>> GetRouteElements(RouteNetworkElementIdList guids);

        IRouteNetworkState NetworkState { get; }
    }
}
