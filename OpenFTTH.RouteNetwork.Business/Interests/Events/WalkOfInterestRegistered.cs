using OpenFTTH.RouteNetwork.API.Model;
using System;

namespace OpenFTTH.RouteNetwork.Business.Interest.Events
{
    public class WalkOfInterestRegistered
    {
        public RouteNetworkInterest Interest { get; }
        public WalkOfInterestRegistered(RouteNetworkInterest interest)
        {
            this.Interest = interest;
        }
    }
}
