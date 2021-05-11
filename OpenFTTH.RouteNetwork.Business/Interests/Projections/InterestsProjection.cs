using FluentResults;
using OpenFTTH.EventSourcing;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.Business.Interest.Events;
using OpenFTTH.RouteNetwork.Business.StateHandling.Interest;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OpenFTTH.RouteNetwork.Business.Interest.Projections
{
    public class InterestsProjection : ProjectionBase
    {
        private readonly ConcurrentDictionary<Guid, RouteNetworkInterest> _interestById = new ConcurrentDictionary<Guid, RouteNetworkInterest>();
        private readonly InMemInterestRelationIndex _interestIndex = new InMemInterestRelationIndex();

        public InterestsProjection()
        {
            ProjectEvent<WalkOfInterestRegistered>(Project);
            ProjectEvent<WalkOfInterestRouteNetworkElementsModified>(Project);
            ProjectEvent<NodeOfInterestRegistered>(Project);
            ProjectEvent<InterestUnregistered>(Project);
        }

        public Result<RouteNetworkInterest> GetInterest(Guid interestId)
        {
            if (_interestById.TryGetValue(interestId, out RouteNetworkInterest? interest))
            {
                return Result.Ok<RouteNetworkInterest>(interest);
            }
            else
            {
                return Result.Fail<RouteNetworkInterest>($"No interest with id: {interestId} found");
            }
        }

        public Result<List<(RouteNetworkInterest, RouteNetworkInterestRelationKindEnum)>> GetInterestsByRouteNetworkElementId(Guid routeNetworkElementId)
        {
            var interestRelations = _interestIndex.GetRouteNetworkElementInterestRelations(routeNetworkElementId);

            List<(RouteNetworkInterest, RouteNetworkInterestRelationKindEnum)> result = new List<(RouteNetworkInterest, RouteNetworkInterestRelationKindEnum)>();

            foreach (var interestRelation in interestRelations)
            {
                result.Add((_interestById[interestRelation.Item1], interestRelation.Item2));
            }

            return Result.Ok<List<(RouteNetworkInterest, RouteNetworkInterestRelationKindEnum)>>(result);
        }


        private void Project(IEventEnvelope eventEnvelope)
        {
            switch (eventEnvelope.Data)
            {
                case (WalkOfInterestRegistered @event):
                    //_interestById[@event.Interest.Id] = @event.Interest;
                    _interestById.TryAdd(@event.Interest.Id, @event.Interest);
                    _interestIndex.Add(@event.Interest);

                    System.Diagnostics.Debug.WriteLine("New walk of interest " + @event.Interest.Id);
                    break;

                case (WalkOfInterestRouteNetworkElementsModified @event):
                    var existingInterest = _interestById[@event.InterestId];
                    var updatedInterest = existingInterest with { RouteNetworkElementRefs = @event.RouteNetworkElementIds };
                    _interestById.TryUpdate(@event.InterestId, updatedInterest, existingInterest);
                    _interestIndex.Update(updatedInterest, existingInterest);

                    System.Diagnostics.Debug.WriteLine("Walk of interest modified " + @event.InterestId);
                    break;

                case (NodeOfInterestRegistered @event):
                    _interestById[@event.Interest.Id] = @event.Interest;
                    _interestIndex.Add(@event.Interest);
                    System.Diagnostics.Debug.WriteLine("New node of interest " + @event.Interest.Id);
                    break;

                case (InterestUnregistered @event):
                    System.Diagnostics.Debug.WriteLine("Interest removed " + @event.InterestId);

                    _interestById.TryGetValue(@event.InterestId, out var routeNetworkInterest);

                    var existingInterestToBeRemoved = _interestById[@event.InterestId];
                    
                    _interestById.TryRemove(@event.InterestId, out _);
                    _interestIndex.Remove(existingInterestToBeRemoved);
                    break;
            }
        }
    }
}
