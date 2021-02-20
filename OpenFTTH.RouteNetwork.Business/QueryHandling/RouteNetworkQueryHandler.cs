using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using OpenFTTH.CQRS;
using OpenFTTH.EventSourcing;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.API.Queries;
using OpenFTTH.RouteNetwork.Business.Interest.Projections;
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
        private readonly IEventStore _eventStore;
        private readonly IRouteNetworkRepository _routeNodeRepository;

        public RouteNetworkQueryHandler(ILoggerFactory loggerFactory, IEventStore eventStore, IRouteNetworkRepository routeNodeRepository)
        {
            if (null == loggerFactory)
            {
                throw new ArgumentNullException("loggerFactory is null");
            }

            _logger = loggerFactory.CreateLogger<RouteNetworkQueryHandler>();

            _eventStore = eventStore;
            _routeNodeRepository = routeNodeRepository;
        }

        public Task<Result<GetRouteNetworkDetailsResult>> HandleAsync(GetRouteNetworkDetails query)
        {
            // Get route elements
            if (query.RouteNetworkElementIdsToQuery.Count > 0 && query.InterestIdsToQuery.Count == 0)
            {
                return QueryByRouteElementIds(query);
            }
            else if (query.InterestIdsToQuery.Count > 0 && query.RouteNetworkElementIdsToQuery.Count == 0)
            {
                return QueryByInterestIds(query);
            }
            else
            {
                if (query.InterestIdsToQuery.Count > 0 && query.RouteNetworkElementIdsToQuery.Count > 0)
                    return Task.FromResult(Result.Failure<GetRouteNetworkDetailsResult>("Invalid query. Cannot query by route network element ids and interest ids at the same time."));
                else if (query.InterestIdsToQuery.Count == 0 && query.RouteNetworkElementIdsToQuery.Count == 0)
                    return Task.FromResult(Result.Failure<GetRouteNetworkDetailsResult>("Invalid query. Neither route network element ids or interest ids specified. Therefore nothing to query."));
                else
                    return Task.FromResult(Result.Failure<GetRouteNetworkDetailsResult>("Invalid query."));
            }
        }

        private Task<Result<GetRouteNetworkDetailsResult>> QueryByInterestIds(GetRouteNetworkDetails query)
        {
            RouteNetworkElementIdList routeElementsToQuery = new RouteNetworkElementIdList();

            List<RouteNetworkInterest> interestsToReturn = new List<RouteNetworkInterest>();

            // Find all interest to return and create a list of route network elements at the same time
            var interestsProjection = _eventStore.Projections.Get<InterestsProjection>();

            foreach (var interestId in query.InterestIdsToQuery)
            {
                var interestQueryResult = interestsProjection.GetInterest(interestId);

                if (interestQueryResult.IsFailure)
                    return Task.FromResult(Result.Failure<GetRouteNetworkDetailsResult>(interestQueryResult.Error));

                interestsToReturn.Add(interestQueryResult.Value);

                routeElementsToQuery.AddRange(interestQueryResult.Value.RouteNetworkElementRefs);
            }

            var getRouteNetworkElementsResult = _routeNodeRepository.GetRouteElements(routeElementsToQuery);

            if (getRouteNetworkElementsResult.IsFailure)
                return Task.FromResult(Result.Failure<GetRouteNetworkDetailsResult>(getRouteNetworkElementsResult.Error));

            var mappedRouteNetworkElements = MapRouteElementDomainObjectsToQueryObjects(query, getRouteNetworkElementsResult.Value);

            var queryResult = new GetRouteNetworkDetailsResult(mappedRouteNetworkElements, interestsToReturn.ToArray());

            // Add interest reference information
            AddInterestReferencesToRouteNetworkElements(query, queryResult);

            return Task.FromResult(
                Result.Success<GetRouteNetworkDetailsResult>(
                    queryResult
                )
            );
        }


        private Task<Result<GetRouteNetworkDetailsResult>> QueryByRouteElementIds(GetRouteNetworkDetails query)
        {
            var getRouteNetworkElementsResult = _routeNodeRepository.GetRouteElements(query.RouteNetworkElementIdsToQuery);

            if (getRouteNetworkElementsResult.IsFailure)
                return Task.FromResult(Result.Failure<GetRouteNetworkDetailsResult>(getRouteNetworkElementsResult.Error));

            var routeNetworkElementsToReturn = MapRouteElementDomainObjectsToQueryObjects(query, getRouteNetworkElementsResult.Value);

            var interestsToReturn = Array.Empty<RouteNetworkInterest>();

            if (query.RelatedInterestFilter == RelatedInterestFilterOptions.ReferencesFromRouteElementAndInterestObjects)
            {
                interestsToReturn = GetInterestsRelatedToRouteNetworkElements(query.RouteNetworkElementIdsToQuery);
            }

            var queryResult = new GetRouteNetworkDetailsResult(routeNetworkElementsToReturn, interestsToReturn);

            // Add interest information
            AddInterestReferencesToRouteNetworkElements(query, queryResult);

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
                var interestsProjection = _eventStore.Projections.Get<InterestsProjection>();

                foreach (var routeElement in queryResult.RouteNetworkElements)
                {
                    // Add relations to the route network element
                    var interestRelationsResult = interestsProjection.GetInterestsByRouteNetworkElementId(routeElement.Id);

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

        private RouteNetworkInterest[] GetInterestsRelatedToRouteNetworkElements(RouteNetworkElementIdList routeNetworkElementIds)
        {
            Dictionary<Guid, RouteNetworkInterest> interestsToBeAddedToResult = new Dictionary<Guid, RouteNetworkInterest>();

            var interestsProjection = _eventStore.Projections.Get<InterestsProjection>();

            foreach (var routeElementId in routeNetworkElementIds)
            {
                // Add relations to the route network element
                var interestRelationsResult = interestsProjection.GetInterestsByRouteNetworkElementId(routeElementId);

                if (interestRelationsResult.IsFailure)
                    throw new ApplicationException($"Unexpected error querying interests related to route network element with id: {routeElementId} {interestRelationsResult.Error}");

                foreach (var interestRelation in interestRelationsResult.Value)
                {
                    if (!interestsToBeAddedToResult.ContainsKey(interestRelation.Item1.Id))
                    {
                        interestsToBeAddedToResult.Add(interestRelation.Item1.Id, interestRelation.Item1);
                    }
                }
            }

            return interestsToBeAddedToResult.Values.ToArray();
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

        private static RouteNetworkElementInterestRelation[] MapInterestRelationDomainObjectsToQueryObjects(List<(RouteNetworkInterest, RouteNetworkInterestRelationKindEnum)> interestRelations)
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


