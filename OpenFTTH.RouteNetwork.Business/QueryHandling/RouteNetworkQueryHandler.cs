using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using OpenFTTH.CQRS;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.API.Queries;
using OpenFTTH.RouteNetwork.Business.DomainModel.Interest;
using OpenFTTH.RouteNetwork.Business.StateHandling;
using OpenFTTH.RouteNetwork.Service.Business.DomainModel.RouteNetwork;
using OpenFTTH.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetworkService.QueryHandlers
{
    public class RouteNetworkQueryHandler :
        IQueryHandler<GetRouteNetworkDetails, Result<GetRouteNetworkDetailsResult>>
    {
        private readonly ILogger<RouteNetworkQueryHandler> _logger;
        private readonly IRouteNetworkRepository _routeNodeRepository;
        private readonly IInterestRepository _interestRepository;

        public RouteNetworkQueryHandler(ILoggerFactory loggerFactory, IRouteNetworkRepository routeNodeRepository, IInterestRepository interestRepository)
        {
            if (null == loggerFactory)
            {
                throw new ArgumentNullException("loggerFactory is null");
            }

            _logger = loggerFactory.CreateLogger<RouteNetworkQueryHandler>();

            _routeNodeRepository = routeNodeRepository;
            _interestRepository = interestRepository;
        }

        public Task<Result<GetRouteNetworkDetailsResult>> HandleAsync(GetRouteNetworkDetails query)
        {
            // Get route elements
            var getRouteNetworkElementsResult = _routeNodeRepository.GetRouteElements(query.RouteNetworkElementIdsToQuery);

            if (getRouteNetworkElementsResult.IsFailure)
                return Task.FromResult(Result.Failure<GetRouteNetworkDetailsResult>(getRouteNetworkElementsResult.Error));

            var mappedRouteNetworkElements = MapRouteElementDomainObjectsToQueryObjects(query, getRouteNetworkElementsResult.Value);

            var queryResult = new GetRouteNetworkDetailsResult(mappedRouteNetworkElements);

            // Add interest information
            AddInterestReferencesToRouteNetworkElements(query, queryResult);
            AddInterestObjectsToQueryResult(query, queryResult);

            return Task.FromResult(
                Result.Success<GetRouteNetworkDetailsResult>(
                    queryResult
                )
            );
        }

        private void AddInterestReferencesToRouteNetworkElements(GetRouteNetworkDetails query, GetRouteNetworkDetailsResult queryResult)
        {
            if (query.RelatedInterestFilter != RelatedInterestFilterOptions.None)
            {
                foreach (var routeElement in queryResult.RouteNetworkElements)
                {
                    // Add relations to the route network element
                    var interestRelationsResult = _interestRepository.GetInterestsByRouteNetworkElementId(routeElement.Id);

                    if (interestRelationsResult.IsFailure)
                        throw new ApplicationException($"Unexpected error querying interests related to route network element with id: {routeElement.Id} {interestRelationsResult.Error}");

                    routeElement.InterestRelations = MapInterestRelationDomainObjectsToQueryObjects(interestRelationsResult.Value);

                    // Add the interest object itself as well if the filter says so
                    if (query.RelatedInterestFilter == RelatedInterestFilterOptions.ReferencesFromRouteElementAndInterestObjects)
                    {
                        
                    }
                }
            }
        }

        private void AddInterestObjectsToQueryResult(GetRouteNetworkDetails query, GetRouteNetworkDetailsResult queryResult)
        {
            // Only add them if request by the caller
            if (query.RelatedInterestFilter == RelatedInterestFilterOptions.ReferencesFromRouteElementAndInterestObjects)
            {
                Dictionary<Guid, RouteNetworkInterest> interestsToBeAddedToResult = new Dictionary<Guid, RouteNetworkInterest>();

                foreach (var routeElement in queryResult.RouteNetworkElements)
                {
                    // Add relations to the route network element
                    var interestRelationsResult = _interestRepository.GetInterestsByRouteNetworkElementId(routeElement.Id);

                    if (interestRelationsResult.IsFailure)
                        throw new ApplicationException($"Unexpected error querying interests related to route network element with id: {routeElement.Id} {interestRelationsResult.Error}");

                    foreach (var interestRelation in interestRelationsResult.Value)
                    {
                        if (!interestsToBeAddedToResult.ContainsKey(interestRelation.Item1.Id))
                        {
                            interestsToBeAddedToResult.Add(interestRelation.Item1.Id, MapInterestObjectToQueryObjects(interestRelation.Item1));
                        }
                    }
                }

                queryResult.Interests = new LookupCollection<RouteNetworkInterest>(interestsToBeAddedToResult.Values.ToArray());
            }
        }

        private RouteNetworkInterest MapInterestObjectToQueryObjects(IInterest interest)
        {
            RouteNetworkInterestKindEnum interestKind = (interest is WalkOfInterest) ? RouteNetworkInterestKindEnum.WalkOfInterest : RouteNetworkInterestKindEnum.NodeOfInterest;

            return new RouteNetworkInterest(interest.Id, interestKind, interest.RouteNetworkElementIds);
        }

        private static RouteNetworkElement[] MapRouteElementDomainObjectsToQueryObjects(GetRouteNetworkDetails query, List<IRouteNetworkElement> routeNetworkElements)
        {
            var routeNetworkElementDTOs = new List<RouteNetworkElement>();

            foreach (var routeNetworkElement in routeNetworkElements)
            {
                RouteNetworkElementKindEnum kind = (routeNetworkElement is IRouteNode) ? RouteNetworkElementKindEnum.RouteNode : RouteNetworkElementKindEnum.RouteSegment;

                routeNetworkElementDTOs.Add(
                    new RouteNetworkElement(routeNetworkElement.Id, kind)
                    {
                        Coordinates = query.RouteNetworkElementFilter.IncludeCoordinates ? routeNetworkElement.Coordinates : null,
                        RouteSegmentInfo = query.RouteNetworkElementFilter.IncludeRouteSegmentInfo && routeNetworkElement is IRouteSegment segment ? segment.RouteSegmentInfo : null,
                        RouteNodeInfo = query.RouteNetworkElementFilter.IncludeRouteNodeInfo && routeNetworkElement is IRouteNode node ? node.RouteNodeInfo : null,
                        NamingInfo = query.RouteNetworkElementFilter.IncludeNamingInfo ? routeNetworkElement.NamingInfo : null,
                        MappingInfo = query.RouteNetworkElementFilter.IncludeMappingInfo ? routeNetworkElement.MappingInfo : null,
                        LifecycleInfo = query.RouteNetworkElementFilter.IncludeLifecycleInfo ? routeNetworkElement.LifecycleInfo : null,
                        SafetyInfo = query.RouteNetworkElementFilter.IncludeSafetyInfo ? routeNetworkElement.SafetyInfo : null,
                    }
                );
            }

            return routeNetworkElementDTOs.ToArray();
        }

        private static RouteNetworkElementInterestRelation[] MapInterestRelationDomainObjectsToQueryObjects(List<(IInterest, RouteNetworkInterestRelationKindEnum)> interestRelations)
        {
            var interestRelationsToReturn = new List<RouteNetworkElementInterestRelation>();

            foreach (var interestRelation in interestRelations)
            {
                interestRelationsToReturn.Add(
                    new RouteNetworkElementInterestRelation(interestRelation.Item1.Id, interestRelation.Item2)
                );
            }

            return interestRelationsToReturn.ToArray();
        }

    }
}


