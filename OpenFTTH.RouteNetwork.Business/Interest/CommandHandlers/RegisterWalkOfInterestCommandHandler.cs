using CSharpFunctionalExtensions;
using OpenFTTH.CQRS;
using OpenFTTH.EventSourcing;
using OpenFTTH.RouteNetwork.API.Commands;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.Business.Interest;
using OpenFTTH.RouteNetwork.Business.Interest.Projections;
using OpenFTTH.RouteNetwork.Business.StateHandling;
using System;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetwork.Business.MutationHandling
{
    public class RegisterWalkOfInterestCommandHandler : ICommandHandler<RegisterWalkOfInterest, Result>
    {
        private readonly IEventStore _eventStore;
        private readonly IRouteNetworkRepository _routeNetworkRepository;

        public RegisterWalkOfInterestCommandHandler(IEventStore eventStore, IRouteNetworkRepository routeNodeRepository)
        {
            _eventStore = eventStore;
            _routeNetworkRepository = routeNodeRepository;
        }

        public Task<Result> HandleAsync(RegisterWalkOfInterest command)
        {
            var interestProjection = _eventStore.Projections.Get<InterestsProjection>();

            var walkOfInterest = new RouteNetworkInterest(command.InterestId, RouteNetworkInterestKindEnum.WalkOfInterest, command.WalkIds);

            var walkValidator = new WalkValidator(_routeNetworkRepository);

            try
            {
                var walkOfInterestAR = new WalkOfInterestAR(walkOfInterest, interestProjection, walkValidator);

                _eventStore.Aggregates.Store(walkOfInterestAR);

                return Task.FromResult(Result.Success());
            }
            catch (ArgumentException ex)
            {
                return Task.FromResult(Result.Failure(ex.Message));
            }
        }
    }
}
