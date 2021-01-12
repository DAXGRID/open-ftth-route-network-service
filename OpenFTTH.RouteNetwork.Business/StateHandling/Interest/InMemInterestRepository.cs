using CSharpFunctionalExtensions;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.Business.DomainModel.Interest;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetwork.Business.StateHandling.Interest
{
    public class InMemInterestRepository : IInterestRepository
    {
        private readonly ConcurrentDictionary<Guid, IInterest> _interestById = new ConcurrentDictionary<Guid, IInterest>();

        private InMemInterestRelationIndex _interestIndex = new InMemInterestRelationIndex();

        public IInterest RegisterWalkOfInterest(Guid interestId, RouteNetworkElementIdList walkIds)
        {
            WalkOfInterest walkOfInterest = new WalkOfInterest(interestId, walkIds);

            if (!_interestById.TryAdd(interestId, walkOfInterest))
                throw new ApplicationException($"Interest with id: {interestId} already exist in repository");

            _interestIndex.AddOrUpdate(walkOfInterest);

            return walkOfInterest;
        }

        public Result<IInterest> GetInterest(Guid interestId)
        {
            if (_interestById.TryGetValue(interestId, out IInterest? interest))
            {
                return Result.Success<IInterest>(interest);
            }
            else
            {
                return Result.Failure<IInterest>($"No interest with id: {interestId} found");
            }
        }

        public Result<List<(IInterest, RouteNetworkInterestRelationKindEnum)>> GetInterestsByRouteNetworkElementId(Guid routeNetworkElementId)
        {
            var interestRelations = _interestIndex.GetRouteNetworkElementInterestRelations(routeNetworkElementId);

            List<(IInterest, RouteNetworkInterestRelationKindEnum)> result = new List<(IInterest, RouteNetworkInterestRelationKindEnum)>();

            foreach (var interestRelation in interestRelations)
            {
                result.Add((_interestById[interestRelation.Item1], interestRelation.Item2));
            }

            return Result.Success<List<(IInterest, RouteNetworkInterestRelationKindEnum)>> (result);
        }
    }
}
