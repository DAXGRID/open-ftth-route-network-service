using OpenFTTH.RouteNetwork.API.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetwork.Business.DomainModel.Interest
{
    public class WalkOfInterest : IInterest
    {
        public Guid Id { get; }

        public RouteNetworkElementIdList RouteNetworkElementIds { get; }

        public WalkOfInterest(Guid id, RouteNetworkElementIdList routeNetworkElementIds)
        {
            this.Id = id;
            this.RouteNetworkElementIds = routeNetworkElementIds;
        }
    }
}
