using CSharpFunctionalExtensions;
using OpenFTTH.RouteNetwork.API.Queries;
using OpenFTTH.RouteNetwork.Business.DomainModel.Interest;
using System;

namespace OpenFTTH.RouteNetwork.Business.StateHandling
{
    public interface IInterestRepository
    {
        Result<IInterest> GetInterest(Guid interestId);
    }
}
