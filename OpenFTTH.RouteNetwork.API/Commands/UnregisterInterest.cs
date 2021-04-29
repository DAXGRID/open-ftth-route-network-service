using FluentResults;
using OpenFTTH.CQRS;
using System;

namespace OpenFTTH.RouteNetwork.API.Commands
{
    public record UnregisterInterest : BaseCommand, ICommand<Result>
    {
        public static string RequestName => typeof(RegisterNodeOfInterest).Name;
        public Guid InterestId { get; }

        public UnregisterInterest(Guid interestId)
        {
            this.CmdId = Guid.NewGuid();
            this.Timestamp = DateTime.UtcNow;

            this.InterestId = interestId;
        }
    }
}
