using FluentResults;
using OpenFTTH.EventSourcing;
using OpenFTTH.RouteNetwork.API.Commands;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.Business.Interest.Events;
using OpenFTTH.RouteNetwork.Business.Interest.Projections;
using System;
using System.Linq;

namespace OpenFTTH.RouteNetwork.Business.Interest
{
    public class WalkOfInterestAR : AggregateBase
    {
        public WalkOfInterestAR()
        {
            Register<WalkOfInterestRegistered>(Apply);
        }

        public Result RegisterWalkOfInterest(RouteNetworkInterest interest, InterestsProjection interestsProjection, WalkValidator walkValidator)
        {
            if (interest.Kind != RouteNetworkInterestKindEnum.WalkOfInterest)
                return Result.Fail(new InterestValidationError(InterestValidationErrorCodes.INTEREST_INVALID_WALK, "Interest kind must be WalkOfInterest"));

            if (interest.Id == Guid.Empty)
                return Result.Fail($"{InterestValidationErrorCodes.INTEREST_INVALID_ID}: Interest id cannot be empty");

            if (interestsProjection.GetInterest(interest.Id).IsSuccess)
                return Result.Fail($"{InterestValidationErrorCodes.INTEREST_ALREADY_EXISTS}: An interest with id: {interest.Id} already exists");

            var walkValidationResult = walkValidator.ValidateWalk(interest.RouteNetworkElementRefs);

            if (walkValidationResult.IsFailed)
                return Result.Fail(walkValidationResult.Errors.First());

            var interestWithValidatedWalk = interest with { RouteNetworkElementRefs = walkValidationResult.Value };

            RaiseEvent(new WalkOfInterestRegistered(interestWithValidatedWalk));

            return Result.Ok();
        }

        private void Apply(WalkOfInterestRegistered obj)
        {
            Id = obj.Interest.Id;
        }
    }
}
