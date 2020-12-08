using OpenFTTH.RouteNetworkService.Queries;

namespace OpenFTTH.RouteNetwork.Business.StateHandling.InMemory
{
    public class InMemRouteNetworkRepository : IRouteNetworkRepository
    {

        public InMemRouteNetworkRepository()
        {

        }

        public RouteNodeQueryResult NodeQuery(RouteNodeQuery query)
        {
            return new RouteNodeQueryResult()
            {
                NamingInfo = new Events.Core.Infos.NamingInfo("grumme hans", "was here")
            };
        }
    }
}
