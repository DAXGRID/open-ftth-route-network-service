using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetworkService.Queries
{
    public class RouteNodeQueryResult
    {
        public Guid RouteNodeId { get; set; }
        public RouteNodeInfo RouteNodeInfo { get; set; }
        public NamingInfo NamingInfo { get; set; }
    }
}
