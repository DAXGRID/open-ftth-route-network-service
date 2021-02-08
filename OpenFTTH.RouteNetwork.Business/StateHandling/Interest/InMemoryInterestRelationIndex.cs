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
    /// <summary>
    /// In-memory index holding relations from route network elements to the interests that starts, ends or pass through them.
    /// Used for fast lookup of all interests related to a given route network element.
    /// </summary>
    public class InMemoryInterestRelationIndex
    {
        private readonly ConcurrentDictionary<Guid, List<(Guid, RouteNetworkInterestRelationKindEnum)>> _routeElementInterestRelations = new ConcurrentDictionary<Guid, List<(Guid, RouteNetworkInterestRelationKindEnum)>>();

        /// <summary>
        /// Add interest to index. If already indexed, the index will be updated.
        /// </summary>
        /// <param name="interest"></param>
        public void AddOrUpdate(IInterest interest)
        {
            // Make all existing index entries referencing the interest
            RemoveExistingInterestIdsFromIndex(interest.Id);

            Add(interest);
        }

        /// <summary>
        /// Add interest to index. If already indexed, eventually old route element references will *not* be removed.
        /// </summary>
        /// <param name="interest"></param>
        public void Add(IInterest interest)
        {
            // Create index entries for all route elements ids covered by the interest
            for (int i = 0; i < interest.RouteNetworkElementIds.Count; i++)
            {
                var currentRouteElementId = interest.RouteNetworkElementIds[i];

                RouteNetworkInterestRelationKindEnum relKind = RouteNetworkInterestRelationKindEnum.Start;

                if (interest.RouteNetworkElementIds.Count == 1)
                    relKind = RouteNetworkInterestRelationKindEnum.InsideNode;
                else if (i == 0)
                    relKind = RouteNetworkInterestRelationKindEnum.Start;
                else if (i == interest.RouteNetworkElementIds.Count - 1)
                    relKind = RouteNetworkInterestRelationKindEnum.End;
                else
                    relKind = RouteNetworkInterestRelationKindEnum.PassThrough;

                if (_routeElementInterestRelations.TryGetValue(currentRouteElementId, out var interestRelationList))
                {
                    if (!interestRelationList.Any(v => v.Item1 == interest.Id)) // we don't want dublicates
                        _routeElementInterestRelations[currentRouteElementId] = MakeCopyOfInterestListAndAddInterest(interestRelationList, (interest.Id, relKind));
                }
                else
                {
                    _routeElementInterestRelations[currentRouteElementId] = new List<(Guid, RouteNetworkInterestRelationKindEnum)>() { (interest.Id, relKind) };
                }
            }
        }

        public void Remove(Guid interestId)
        {
            RemoveExistingInterestIdsFromIndex(interestId);
        }

        public List<(Guid, RouteNetworkInterestRelationKindEnum)> GetRouteNetworkElementInterestRelations(Guid routeElementId)
        {
            if (_routeElementInterestRelations.TryGetValue(routeElementId, out var interestRelationList))
            {
                return interestRelationList;
            }
            else
            {
                return new List<(Guid, RouteNetworkInterestRelationKindEnum)>();
            }
        }

        private void RemoveExistingInterestIdsFromIndex(Guid interestId)
        {
            // We iterate through every route element to interest relation in the index using old fashioned foreach loops to be most CPU efficient
            // One could avoid this to by having som additional index from interest to route elements, but that would require more memory
            foreach (var interestRelationList in _routeElementInterestRelations)
            {
                bool routeElementContainsInterestRelation = false;

                foreach (var interestRelation in interestRelationList.Value)
                {
                    if (interestRelation.Item1 == interestId)
                    {
                        routeElementContainsInterestRelation = true;
                    }
                }

                if (routeElementContainsInterestRelation)
                {
                    // We need create new interest list by copying existing one and removing the element, to be thread safe, 
                    _routeElementInterestRelations[interestRelationList.Key] = MakeCopyOfInterestListAndRemovedInterest(interestRelationList.Value, interestId);
                }
            }
        }

        private static List<(Guid, RouteNetworkInterestRelationKindEnum)> MakeCopyOfInterestListAndRemovedInterest(List<(Guid, RouteNetworkInterestRelationKindEnum)> listToCopy, Guid interestToRemove)
        {
            List<(Guid, RouteNetworkInterestRelationKindEnum)> newListWithRemovedInterest = new List<(Guid, RouteNetworkInterestRelationKindEnum)>();

            foreach (var interestRel in listToCopy)
            {
                if (interestRel.Item1 != interestToRemove)
                    newListWithRemovedInterest.Add(interestRel);
            }

            return newListWithRemovedInterest;
        }

        private static List<(Guid, RouteNetworkInterestRelationKindEnum)> MakeCopyOfInterestListAndAddInterest(List<(Guid, RouteNetworkInterestRelationKindEnum)> listToCopy, (Guid, RouteNetworkInterestRelationKindEnum) interestRelationToAdd)
        {
            List<(Guid, RouteNetworkInterestRelationKindEnum)> newListWithAddedInterest = new List<(Guid, RouteNetworkInterestRelationKindEnum)>();

            foreach (var interestRel in listToCopy)
            {
                newListWithAddedInterest.Add(interestRel);
            }

            newListWithAddedInterest.Add(interestRelationToAdd);

            return newListWithAddedInterest;
        }
    }
}
