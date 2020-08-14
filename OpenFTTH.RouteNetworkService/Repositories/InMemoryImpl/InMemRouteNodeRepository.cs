using OpenFTTH.RouteNetworkService.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetworkService.Repositories.InMemoryImpl
{
    public class InMemRouteNodeRepository : IRouteNodeRepository
    {

        public InMemRouteNodeRepository()
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
