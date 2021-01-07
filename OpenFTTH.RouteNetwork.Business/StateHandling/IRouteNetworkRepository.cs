using CSharpFunctionalExtensions;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.API.Queries;
using OpenFTTH.RouteNetwork.Business.DomainModel.Interest;

namespace OpenFTTH.RouteNetwork.Business.StateHandling
{
    public interface IRouteNetworkRepository
    {
        Result<GetRouteNetworkDetailsQueryResult> GetRouteElements(GetRouteNetworkDetailsQuery query);

        IRouteNetworkState NetworkState { get; }
    }
}
