using OpenFTTH.RouteNetwork.API.Model;
using System;

namespace OpenFTTH.RouteNetwork.Business.Interest.Events
{
    public class NodeOfInterestRegistered
    {
        public RouteNetworkInterest Interest { get; }
        public NodeOfInterestRegistered(RouteNetworkInterest interest)
        {
            this.Interest = interest;
        }
    }
}
