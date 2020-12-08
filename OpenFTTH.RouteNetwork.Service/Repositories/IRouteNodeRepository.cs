using OpenFTTH.RouteNetworkService.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetworkService.Repositories
{
    public interface IRouteNodeRepository
    {
        RouteNodeQueryResult Query(RouteNodeQuery query);
    }
}
