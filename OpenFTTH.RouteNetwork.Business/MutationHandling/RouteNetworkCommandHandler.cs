using CSharpFunctionalExtensions;
using OpenFTTH.CQRS;
using OpenFTTH.RouteNetwork.API.Mutations;
using OpenFTTH.RouteNetwork.Business.StateHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetwork.Business.MutationHandling
{
    public class RouteNetworkCommandHandler :
        ICommandHandler<RegisterWalkOfInterestCommand, Result<RegisterWalkOfInterestCommandResult>>
    {
        private readonly IRouteNetworkRepository _routeNetworkRepository;
        private readonly IInterestRepository _interestRepository;

        public RouteNetworkCommandHandler(IRouteNetworkRepository routeNodeRepository, IInterestRepository interestRepository)
        {
            _routeNetworkRepository = routeNodeRepository;
            _interestRepository = interestRepository;
        }

        public Task<Result<RegisterWalkOfInterestCommandResult>> HandleAsync(RegisterWalkOfInterestCommand command)
        {
            // Make sure that an interest with the specified id not already exists in the system
            if (_interestRepository.GetInterest(command.InterestId).IsSuccess)
                return Task.FromResult(Result.Failure<RegisterWalkOfInterestCommandResult>($"An interest with id: {command.InterestId} already exists"));

            // Validate the walk
            var walkValidationResult = new WalkValidator(_routeNetworkRepository).ValidateWalk(command.WalkIds);

            if (walkValidationResult.IsFailure)
                return Task.FromResult(Result.Failure<RegisterWalkOfInterestCommandResult>(walkValidationResult.Error));

            // If we get here everything should be good, so return the result
            var registerWalkOfInterestCommandResult = new RegisterWalkOfInterestCommandResult(walkValidationResult.Value.WalkIds);

            return Task.FromResult(Result.Success<RegisterWalkOfInterestCommandResult>(registerWalkOfInterestCommandResult));
        }
    }
}
