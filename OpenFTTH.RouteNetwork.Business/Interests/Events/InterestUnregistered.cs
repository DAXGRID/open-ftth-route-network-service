using System;

namespace OpenFTTH.RouteNetwork.Business.Interest.Events
{
    public class InterestUnregistered
    {
        public Guid InterestId { get; }
        public InterestUnregistered(Guid interestId)
        {
            this.InterestId = interestId;
        }
    }
}
