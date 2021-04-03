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
    public class InterestAR : AggregateBase
    {
        private RouteNetworkInterest? _interest;
        private bool _unregistered = false;

        public InterestAR()
        {
            Register<WalkOfInterestRegistered>(Apply);
            Register<WalkOfInterestRouteNetworkElementsModified>(Apply);
            Register<NodeOfInterestRegistered>(Apply);
            Register<InterestUnregistered>(Apply);
        }

        public Result<RouteNetworkInterest> RegisterWalkOfInterest(RouteNetworkInterest interest, InterestsProjection interestsProjection, WalkValidator walkValidator)
        {
            if (interest.Kind != RouteNetworkInterestKindEnum.WalkOfInterest)
                return Result.Fail(new RegisterWalkOfInterestError(RegisterWalkOfInterestErrorCodes.INVALID_INTEREST_KIND_MUST_BE_WALK_OF_INTEREST, "Interest kind must be WalkOfInterest"));

            if (interest.Id == Guid.Empty)
                return Result.Fail(new RegisterWalkOfInterestError(RegisterWalkOfInterestErrorCodes.INVALID_INTEREST_ID_CANNOT_BE_EMPTY, $"{RegisterWalkOfInterestErrorCodes.INVALID_INTEREST_ID_CANNOT_BE_EMPTY}: Interest id cannot be empty"));

            if (interestsProjection.GetInterest(interest.Id).IsSuccess)
                return Result.Fail(new RegisterWalkOfInterestError(RegisterWalkOfInterestErrorCodes.INVALID_INTEREST_ALREADY_EXISTS, $"{RegisterWalkOfInterestErrorCodes.INVALID_INTEREST_ALREADY_EXISTS}: An interest with id: {interest.Id} already exists"));

            var walkValidationResult = walkValidator.ValidateWalk(interest.RouteNetworkElementRefs);

            if (walkValidationResult.IsFailed)
                return Result.Fail(walkValidationResult.Errors.First());

            var interestWithValidatedWalk = interest with { RouteNetworkElementRefs = walkValidationResult.Value };

            RaiseEvent(new WalkOfInterestRegistered(interestWithValidatedWalk));

            if (_interest == null)
                throw new ApplicationException("Unexpected aggreagate state. Interest must be non-null after WalkOfInterestRegistered has been raised.");

            return Result.Ok<RouteNetworkInterest>(_interest);
        }

        public Result<RouteNetworkInterest> UpdateRouteNetworkElements(RouteNetworkInterest walkOfInterest, InterestsProjection interestProjection, WalkValidator walkValidator)
        {
            if (walkOfInterest.Kind != RouteNetworkInterestKindEnum.WalkOfInterest)
                return Result.Fail(new RegisterWalkOfInterestError(RegisterWalkOfInterestErrorCodes.INVALID_INTEREST_KIND_MUST_BE_WALK_OF_INTEREST, "Interest kind must be WalkOfInterest"));

            if (walkOfInterest.Id == Guid.Empty)
                return Result.Fail(new RegisterWalkOfInterestError(RegisterWalkOfInterestErrorCodes.INVALID_INTEREST_ID_CANNOT_BE_EMPTY, $"{RegisterWalkOfInterestErrorCodes.INVALID_INTEREST_ID_CANNOT_BE_EMPTY}: Interest id cannot be empty"));

            if (interestProjection.GetInterest(walkOfInterest.Id).IsFailed)
                return Result.Fail(new RegisterWalkOfInterestError(RegisterWalkOfInterestErrorCodes.INVALID_INTEREST_DONT_EXISTS, $"{RegisterWalkOfInterestErrorCodes.INVALID_INTEREST_DONT_EXISTS}: An interest with id: {walkOfInterest.Id} could not be found"));

            var walkValidationResult = walkValidator.ValidateWalk(walkOfInterest.RouteNetworkElementRefs);

            if (walkValidationResult.IsFailed)
                return Result.Fail(walkValidationResult.Errors.First());

            RaiseEvent(new WalkOfInterestRouteNetworkElementsModified(this.Id, walkValidationResult.Value));

            if (_interest == null)
                throw new ApplicationException("Unexpected aggreagate state. Interest must be non-null after WalkOfInterestRegistered has been raised.");

            return Result.Ok<RouteNetworkInterest>(_interest);
        }


        public Result UnregisterInterest(InterestsProjection interestsProjection, Guid interestId)
        {
            if (!interestsProjection.GetInterest(interestId).IsSuccess)
                return Result.Fail($"Cannot find interest with id: {interestId}");

            RaiseEvent(new InterestUnregistered(interestId));

            return Result.Ok();
        }

        public Result<RouteNetworkInterest> RegisterNodeOfInterest(RouteNetworkInterest interest, InterestsProjection interestsProjection)
        {
            if (interest.Kind != RouteNetworkInterestKindEnum.NodeOfInterest)
                return Result.Fail(new RegisterNodeOfInterestError(RegisterNodeOfInterestErrorCodes.INVALID_INTEREST_KIND_MUST_BE_NODE_OF_INTEREST, "Interest kind must be NodeOfInterest"));

            if (interest.Id == Guid.Empty)
                return Result.Fail(new RegisterNodeOfInterestError(RegisterNodeOfInterestErrorCodes.INVALID_INTEREST_ID_CANNOT_BE_EMPTY, $"{RegisterWalkOfInterestErrorCodes.INVALID_INTEREST_ID_CANNOT_BE_EMPTY}: Interest id cannot be empty"));

            if (interestsProjection.GetInterest(interest.Id).IsSuccess)
                return Result.Fail(new RegisterNodeOfInterestError(RegisterNodeOfInterestErrorCodes.INVALID_INTEREST_ALREADY_EXISTS, $"{RegisterNodeOfInterestErrorCodes.INVALID_INTEREST_ALREADY_EXISTS}: An interest with id: {interest.Id} already exists"));

            RaiseEvent(new NodeOfInterestRegistered(interest));

            if (_interest == null)
                throw new ApplicationException("Unexpected aggreagate state. Interest must be non-null after NodeOfInterestRegistered has been raised.");

            return Result.Ok<RouteNetworkInterest>(_interest);
        }

        private void Apply(WalkOfInterestRegistered obj)
        {
            Id = obj.Interest.Id;
            _interest = obj.Interest;
        }

        private void Apply(WalkOfInterestRouteNetworkElementsModified @event)
        {
            if (_interest == null)
                throw new ApplicationException("Unexpected aggreagate state. Interest state must be non-null when this event is processed.");

            _interest = _interest with { RouteNetworkElementRefs = @event.RouteNetworkElementIds };
        }

        private void Apply(NodeOfInterestRegistered obj)
        {
            Id = obj.Interest.Id;
            _interest = obj.Interest;
        }

        private void Apply(InterestUnregistered obj)
        {
            _unregistered = true;
        }
    }
}
