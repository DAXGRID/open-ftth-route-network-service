using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.Util;

namespace OpenFTTH.RouteNetwork.API.Queries
{
    public record GetRouteNetworkDetailsResult
    {
        public LookupCollection<RouteNetworkElement> RouteNetworkElements { get; }

        public LookupCollection<RouteNetworkInterest>? Interests { get; set; }

        public GetRouteNetworkDetailsResult(RouteNetworkElement[] routeNetworkElements)
        {
            this.RouteNetworkElements = new LookupCollection<RouteNetworkElement>(routeNetworkElements);
        }
    }
}
