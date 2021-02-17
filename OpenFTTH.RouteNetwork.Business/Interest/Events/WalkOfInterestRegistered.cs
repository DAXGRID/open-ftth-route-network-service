using OpenFTTH.RouteNetwork.API.Model;
using System;

namespace OpenFTTH.RouteNetwork.Business.Interest.Events
{
    public class WalkOfInterestRegistered
    {
        public Guid walkOfInterestId { get; } 
        public RouteNetworkElementIdList routeNetworkWalkIds { get; }

        public WalkOfInterestRegistered(Guid walkOfInterestId, RouteNetworkElementIdList routeNetworkWalkIds)
        {
            this.walkOfInterestId = walkOfInterestId;
            this.routeNetworkWalkIds = routeNetworkWalkIds;
        }
    }
}
