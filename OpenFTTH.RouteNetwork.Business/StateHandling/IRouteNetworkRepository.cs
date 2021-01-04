using CSharpFunctionalExtensions;
using OpenFTTH.RouteNetwork.API.Queries;

namespace OpenFTTH.RouteNetwork.Business.StateHandling
{
    public interface IRouteNetworkRepository
    {
        Result<GetRouteNetworkDetailsQueryResult> GetRouteElements(GetRouteNetworkDetailsQuery query);
    }
}
