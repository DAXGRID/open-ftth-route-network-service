using OpenFTTH.EventSourcing;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.Business.Interest.Events;
using OpenFTTH.RouteNetwork.Business.Interest.Projections;
using OpenFTTH.RouteNetwork.Business.MutationHandling;
using System;

namespace OpenFTTH.RouteNetwork.Business.Interest
{
    public class WalkOfInterestAR : AggregateBase
    {
        public WalkOfInterestAR()
        {
            Register<WalkOfInterestRegistered>(Apply);
        }

        public WalkOfInterestAR(RouteNetworkInterest interest, InterestsProjection interestsProjection, WalkValidator walkValidator) : this()
        {

            if (interest.Kind != RouteNetworkInterestKindEnum.WalkOfInterest)
                throw new ArgumentException("Interest kind must be WalkOfInterest");

            if (interest.Id == Guid.Empty)
                throw new ArgumentException("Interest id cannot be empty");

            if (interestsProjection.GetInterest(interest.Id).IsSuccess)
                throw new ArgumentException($"An interest with id: {interest.Id} already exists");

            var walkValidationResult = walkValidator.ValidateWalk(interest.RouteNetworkElementRefs);

            if (walkValidationResult.IsFailure)
                throw new ArgumentException(walkValidationResult.Error);

            var interestWithValidatedWalk = interest with { RouteNetworkElementRefs = walkValidationResult.Value };

            RaiseEvent(new WalkOfInterestRegistered(interestWithValidatedWalk));
        }

        private void Apply(WalkOfInterestRegistered obj)
        {
            Id = obj.Interest.Id;
        }
    }
}
