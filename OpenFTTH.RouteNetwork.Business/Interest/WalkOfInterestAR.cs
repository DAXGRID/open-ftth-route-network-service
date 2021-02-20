using CSharpFunctionalExtensions;
using OpenFTTH.EventSourcing;
using OpenFTTH.RouteNetwork.API.Commands;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.Business.Interest.Events;
using OpenFTTH.RouteNetwork.Business.Interest.Projections;
using System;

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
                return Result.Failure($"{InterestErrorCodes.INTEREST_INVALID_KIND}: Interest kind must be WalkOfInterest");

            if (interest.Id == Guid.Empty)
                return Result.Failure($"{InterestErrorCodes.INTEREST_INVALID_ID}: Interest id cannot be empty");

            if (interestsProjection.GetInterest(interest.Id).IsSuccess)
                return Result.Failure($"{InterestErrorCodes.INTEREST_ALREADY_EXISTS}: An interest with id: {interest.Id} already exists");

            var walkValidationResult = walkValidator.ValidateWalk(interest.RouteNetworkElementRefs);

            if (walkValidationResult.IsFailure)
                return Result.Failure($"{InterestErrorCodes.INTEREST_INVALID_WALK}: {walkValidationResult.Error}");

            var interestWithValidatedWalk = interest with { RouteNetworkElementRefs = walkValidationResult.Value };

            RaiseEvent(new WalkOfInterestRegistered(interestWithValidatedWalk));

            return Result.Success();
        }

        private void Apply(WalkOfInterestRegistered obj)
        {
            Id = obj.Interest.Id;
        }
    }
}
