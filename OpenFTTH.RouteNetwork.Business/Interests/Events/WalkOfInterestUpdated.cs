using OpenFTTH.RouteNetwork.API.Model;
using System;

namespace OpenFTTH.RouteNetwork.Business.Interest.Events
{
    public class WalkOfInterestRouteNetworkElementsModified
    {
        public Guid InterestId { get; }
        public RouteNetworkElementIdList RouteNetworkElementIds { get; }

        public WalkOfInterestRouteNetworkElementsModified(Guid interestId, RouteNetworkElementIdList routeNetworkElementIds)
        {
            InterestId = interestId;
            RouteNetworkElementIds = routeNetworkElementIds;
        }
    }
}
