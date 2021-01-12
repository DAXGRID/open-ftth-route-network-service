using OpenFTTH.RouteNetwork.API.Model;

namespace OpenFTTH.RouteNetwork.API.Queries
{
    public record GetRouteNetworkDetailsQueryResult
    {
        public RouteNetworkElement[] RouteNetworkElements { get; }

        public RouteNetworkInterest[]? Interests { get; set; }

        public GetRouteNetworkDetailsQueryResult(RouteNetworkElement[] routeNetworkElements)
        {
            this.RouteNetworkElements = routeNetworkElements;
        }
    }
}
