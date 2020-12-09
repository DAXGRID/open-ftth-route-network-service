using OpenFTTH.RouteNetwork.Business.StateHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetwork.Business.MutationHandling
{
    public class RouteNetworkMutationHandler
    {
        private readonly IRouteNetworkRepository _routeNodeRepository;

        public RouteNetworkMutationHandler(IRouteNetworkRepository routeNodeRepository)
        {
            _routeNodeRepository = routeNodeRepository;
        }

    }
}
