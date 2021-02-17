using OpenFTTH.EventSourcing;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.Business.Interest.Events;
using System;

namespace OpenFTTH.RouteNetwork.Business.Interest
{
    public class WalkOfInterestAR : AggregateBase
    {
        public WalkOfInterestAR(Guid walkOfInterestId, RouteNetworkElementIdList routeNetworkWalkIds)
        {
            RaiseEvent(new WalkOfInterestRegistered(walkOfInterestId, routeNetworkWalkIds));

        }
    }
}
