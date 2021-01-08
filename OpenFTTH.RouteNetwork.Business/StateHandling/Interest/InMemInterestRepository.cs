using CSharpFunctionalExtensions;
using OpenFTTH.RouteNetwork.Business.DomainModel.Interest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetwork.Business.StateHandling.Interest
{
    public class InMemInterestRepository : IInterestRepository
    {
        private readonly Dictionary<Guid, IInterest> _interestById = new Dictionary<Guid, IInterest>();

        private InMemInterestRelationIndex _interestIndex = new InMemInterestRelationIndex();

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

        public Result<List<(IInterest, InterestRelationKindEnum)>> GetInterestsByRouteNetworkElementId(Guid routeNetworkElementId)
        {
            var interestRelations = _interestIndex.GetRouteNetworkElementInterestRelations(routeNetworkElementId);

            List<(IInterest, InterestRelationKindEnum)> result = new List<(IInterest, InterestRelationKindEnum)>();

            foreach (var interestRelation in interestRelations)
            {
                result.Add((_interestById[interestRelation.Item1], interestRelation.Item2));
            }

            return Result.Success<List<(IInterest, InterestRelationKindEnum)>> (result);
        }
    }
}
