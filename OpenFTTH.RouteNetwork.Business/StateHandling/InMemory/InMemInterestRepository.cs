using CSharpFunctionalExtensions;
using OpenFTTH.RouteNetwork.Business.DomainModel.Interest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetwork.Business.StateHandling.InMemory
{
    public class InMemInterestRepository : IInterestRepository
    {
        private readonly Dictionary<Guid, IInterest> _interestById = new Dictionary<Guid, IInterest>();

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
    }
}
