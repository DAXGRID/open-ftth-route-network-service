using FluentResults;
using OpenFTTH.CQRS;
using System;

namespace OpenFTTH.RouteNetwork.API.Commands
{
    public class UnregisterInterest : ICommand<Result>
    {
        public static string RequestName => typeof(RegisterNodeOfInterest).Name;
        public Guid InterestId { get; }

        public UnregisterInterest(Guid interestId)
        {
            this.InterestId = interestId;
        }
    }
}
