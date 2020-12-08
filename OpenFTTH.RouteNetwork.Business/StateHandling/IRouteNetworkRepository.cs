using OpenFTTH.RouteNetworkService.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetwork.Business.StateHandling
{
    public interface IRouteNetworkRepository
    {
        RouteNodeQueryResult NodeQuery(RouteNodeQuery query);


    }
}
