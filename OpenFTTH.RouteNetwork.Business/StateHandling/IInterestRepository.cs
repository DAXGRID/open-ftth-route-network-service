using CSharpFunctionalExtensions;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.API.Queries;
using OpenFTTH.RouteNetwork.Business.DomainModel.Interest;
using System;
using System.Collections.Generic;

namespace OpenFTTH.RouteNetwork.Business.StateHandling
{
    public interface IInterestRepository
    {
        IInterest RegisterWalkOfInterest(Guid interestId, RouteNetworkElementIdList walkIds);
        Result<IInterest> GetInterest(Guid interestId);
        Result<List<(IInterest, RouteNetworkInterestRelationKindEnum)>> GetInterestsByRouteNetworkElementId(Guid routeNetworkElementId);
    }
}
