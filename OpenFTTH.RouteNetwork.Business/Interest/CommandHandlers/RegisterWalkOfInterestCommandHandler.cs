using CSharpFunctionalExtensions;
using OpenFTTH.CQRS;
using OpenFTTH.RouteNetwork.API.Commands;
using OpenFTTH.RouteNetwork.Business.StateHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetwork.Business.MutationHandling
{
    public class RegisterWalkOfInterestCommandHandler : ICommandHandler<RegisterWalkOfInterest, Result>
    {
        private readonly IRouteNetworkRepository _routeNetworkRepository;
        private readonly IInterestRepository _interestRepository;

        public RegisterWalkOfInterestCommandHandler(IRouteNetworkRepository routeNodeRepository, IInterestRepository interestRepository)
        {
            _routeNetworkRepository = routeNodeRepository;
            _interestRepository = interestRepository;
        }

        public Task<Result> HandleAsync(RegisterWalkOfInterest command)
        {
            // Make sure that an interest with the specified id not already exists in the system
            if (_interestRepository.GetInterest(command.InterestId).IsSuccess)
                return Task.FromResult(Result.Failure($"An interest with id: {command.InterestId} already exists"));

            // Validate the walk
            var walkValidationResult = new WalkValidator(_routeNetworkRepository).ValidateWalk(command.WalkIds);

            if (walkValidationResult.IsFailure)
                return Task.FromResult(Result.Failure(walkValidationResult.Error));

            // Save the interest
            _interestRepository.RegisterWalkOfInterest(command.InterestId, walkValidationResult.Value.WalkIds);

            // Return command result
            var registerWalkOfInterestCommandResult = new RegisterWalkOfInterestCommandResult(walkValidationResult.Value.WalkIds);
            return Task.FromResult(Result.Success());
        }
    }
}
