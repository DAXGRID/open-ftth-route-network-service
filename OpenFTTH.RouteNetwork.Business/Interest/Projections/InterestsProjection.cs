using CSharpFunctionalExtensions;
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
        }

        public Result<RouteNetworkInterest> GetInterest(Guid interestId)
        {
            if (_interestById.TryGetValue(interestId, out RouteNetworkInterest? interest))
            {
                return Result.Success<RouteNetworkInterest>(interest);
            }
            else
            {
                return Result.Failure<RouteNetworkInterest>($"No interest with id: {interestId} found");
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

            return Result.Success<List<(RouteNetworkInterest, RouteNetworkInterestRelationKindEnum)>>(result);
        }


        private void Project(IEventEnvelope eventEnvelope)
        {
            switch (eventEnvelope.Data)
            {
                case (WalkOfInterestRegistered @event):
                    _interestById[@event.Interest.Id] = @event.Interest;

                    _interestIndex.AddOrUpdate(@event.Interest);
                    break;
            }
        }
    }
}
