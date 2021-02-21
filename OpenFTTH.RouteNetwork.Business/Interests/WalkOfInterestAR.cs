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
        private RouteNetworkInterest? _walkOfInterest;

        public WalkOfInterestAR()
        {
            Register<WalkOfInterestRegistered>(Apply);
        }

        public Result<RouteNetworkInterest> RegisterWalkOfInterest(RouteNetworkInterest interest, InterestsProjection interestsProjection, WalkValidator walkValidator)
        {
            if (interest.Kind != RouteNetworkInterestKindEnum.WalkOfInterest)
                return Result.Fail(new RegisterWalkOfInterestError(RegisterWalkOfInterestErrorCodes.INVALID_INTEREST_KIND_MUST_BE_WALK_OF_INTEREST, "Interest kind must be WalkOfInterest"));

            if (interest.Id == Guid.Empty)
                return Result.Fail($"{RegisterWalkOfInterestErrorCodes.INVALID_INTEREST_ID_CANNOT_BE_EMPTY}: Interest id cannot be empty");

            if (interestsProjection.GetInterest(interest.Id).IsSuccess)
                return Result.Fail($"{RegisterWalkOfInterestErrorCodes.INVALID_INTEREST_ALREADY_EXISTS}: An interest with id: {interest.Id} already exists");

            var walkValidationResult = walkValidator.ValidateWalk(interest.RouteNetworkElementRefs);

            if (walkValidationResult.IsFailed)
                return Result.Fail(walkValidationResult.Errors.First());

            var interestWithValidatedWalk = interest with { RouteNetworkElementRefs = walkValidationResult.Value };

            RaiseEvent(new WalkOfInterestRegistered(interestWithValidatedWalk));

            if (_walkOfInterest == null)
                throw new ApplicationException("Unexpected aggreagate state. Walk of interest must be non-null after WalkOfInterestRegistered has been raised.");

            return Result.Ok<RouteNetworkInterest>(_walkOfInterest);
        }

        private void Apply(WalkOfInterestRegistered obj)
        {
            Id = obj.Interest.Id;
            _walkOfInterest = obj.Interest;
        }
    }
}
