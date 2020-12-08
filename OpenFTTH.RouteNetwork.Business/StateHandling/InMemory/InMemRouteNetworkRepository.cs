using OpenFTTH.RouteNetworkService.Queries;

namespace OpenFTTH.RouteNetwork.Business.StateHandling.InMemory
{
    public class InMemRouteNetworkRepository : IRouteNodeNetworkRepository
    {

        public InMemRouteNetworkRepository()
        {

        }

        public RouteNodeQueryResult Query(RouteNodeQuery query)
        {
            return new RouteNodeQueryResult()
            {
                NamingInfo = new Events.Core.Infos.NamingInfo("grumme hans", "was here")
            };
        }
    }
}
