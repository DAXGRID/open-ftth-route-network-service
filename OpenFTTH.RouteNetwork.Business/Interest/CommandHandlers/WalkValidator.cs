using CSharpFunctionalExtensions;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.Business.DomainModel.Interest;
using OpenFTTH.RouteNetwork.Business.StateHandling;
using OpenFTTH.RouteNetwork.Service.Business.DomainModel.RouteNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetwork.Business.MutationHandling
{
    public class WalkValidator
    {
        readonly IRouteNetworkRepository _routeNetworkRepository;

        public WalkValidator(IRouteNetworkRepository routeNetworkRepository)
        {
            _routeNetworkRepository = routeNetworkRepository;
        }

        public Result<Walk> ValidateWalk(RouteNetworkElementIdList walkIds)
        {
            long versionId = _routeNetworkRepository.NetworkState.GetLatestCommitedVersion();

            var routeNetworkObjects = LookupRouteNetworkObjects(walkIds, versionId);

            // If some route network element could not be looked up then return failure
            if (routeNetworkObjects.IsFailure)
                return Result.Failure<Walk>(routeNetworkObjects.Error);

            // If only one id is specified, make sure it'a a route segment
            if (routeNetworkObjects.Value.Count == 1 && !(routeNetworkObjects.Value[0] is IRouteSegment))
                return Result.Failure<Walk>("If only one route network id is specified in a walk, it must be a route segment id");

            var routeElementsSummary = GetRouteNetworkElementsListSummary(routeNetworkObjects.Value);

            switch (routeElementsSummary)
            {
                case RouteElementListSummary.None:
                    return Result.Failure<Walk>("A valid walk should contain at least one route segment id");

                case RouteElementListSummary.RouteNodesOnly:
                    return Result.Failure<Walk>("A valid walk cannot contain route nodes only. This is because multiple segments might be connecting the same two nodes.");

                case RouteElementListSummary.RouteSegmentsOnly:
                    var routeSegments = routeNetworkObjects.Value.OfType<RouteSegment>().ToList();
                    return ValidateSegmentSequence(routeSegments, versionId);

                case RouteElementListSummary.BothRouteNodesAndSegments:
                    return Result.Failure<Walk>("A valid walk should contain at least one route segment id");
            }

            return Result.Failure<Walk>("Unsupported type of route network id sequence");
        }


        private Result<Walk> ValidateSegmentSequence(List<RouteSegment> routeSegments, long versionId)
        {
            var routeSegmentPosition = 0;

            foreach (var routeSegment in routeSegments)
            {
                if (routeSegmentPosition > 0)
                {
                    var prevSegment = routeSegments[routeSegmentPosition - 1];

                    if (!IsAdjacent(prevSegment, routeSegment, versionId))
                        return Result.Failure<Walk>($"Segments is out of sequence. Segment with id: {routeSegment.Id} was expected to follow segment with id: {prevSegment.Id} but was not.");
                }

                routeSegmentPosition++;
            }

            return Result.Success<Walk>(CreateWalkFromSegmentSequence(routeSegments, versionId));
        }

        private Walk CreateWalkFromSegmentSequence(List<RouteSegment> routeSegments, long versionId)
        {
            RouteNetworkElementIdList walkIds = new RouteNetworkElementIdList();

            var routeSegmentPosition = 0;

            // If only one route segment, inV and outV will be the node sequence
            if (routeSegments.Count == 1)
            {
                walkIds.Add(routeSegments[0].InV(versionId).Id);
                walkIds.Add(routeSegments[0].Id);
                walkIds.Add(routeSegments[0].OutV(versionId).Id);

                return new Walk(walkIds);
            }

            // We're dealing with multi segments, if we reach this code
            foreach (var routeSegment in routeSegments)
            {
                if (routeSegmentPosition > 0)
                {
                    var prevSegment = routeSegments[routeSegmentPosition - 1];

                    var sharedRouteNode = FindSharedNode(prevSegment, routeSegment, versionId);

                    // If prevSegment is the first segment then remember to add it incl. the first node and shared node
                    if (routeSegmentPosition == 1)
                    {
                        if (prevSegment.InV(versionId) != sharedRouteNode)
                            walkIds.Add(prevSegment.InV(versionId).Id);
                        else
                            walkIds.Add(prevSegment.OutV(versionId).Id);

                        // Add prev segment
                        walkIds.Add(prevSegment.Id);

                        // Add shared node
                        walkIds.Add(sharedRouteNode.Id);
                    }

                    // Add current segment and non shared node
                    walkIds.Add(routeSegment.Id);

                    if (routeSegment.InV(versionId) != sharedRouteNode)
                        walkIds.Add(routeSegment.InV(versionId).Id);
                    else
                        walkIds.Add(routeSegment.OutV(versionId).Id);
                }

                routeSegmentPosition++;
            }

            return new Walk(walkIds);
        }

        private RouteNode FindSharedNode(RouteSegment segment1, RouteSegment segment2, long versionId)
        {
            foreach (var neighborNode in segment1.NeighborElements(versionId))
            {
                if (segment2.NeighborElements(versionId).Contains(neighborNode))
                    return (RouteNode)neighborNode;
            }

            throw new ApplicationException("FindSharedNode should never be called on an unvalidated pair of segments.");
        }

        private static bool IsAdjacent(RouteSegment segment1, RouteSegment segment2, long versionId)
        {
            foreach (var neighboor in segment1.NeighborElements(versionId))
            {
                if (neighboor.NeighborElements(versionId).Contains(segment2))
                    return true;
            }

            return false;
        }

        private Result<List<IRouteNetworkElement>> LookupRouteNetworkObjects(RouteNetworkElementIdList routeNetworkElementIds, long versionId)
        {
            List<IRouteNetworkElement> result = new List<IRouteNetworkElement>();

            foreach (var networkElementId in routeNetworkElementIds)
            {
                var routeNetworkElement = _routeNetworkRepository.NetworkState.GetRouteNetworkElement(networkElementId, versionId);

                if (routeNetworkElement == null)
                    return Result.Failure<List<IRouteNetworkElement>>($"Cannot find any route network element with id: {networkElementId}");
                else
                    result.Add(routeNetworkElement);
            }

            return Result.Success<List<IRouteNetworkElement>>(result);
        }

        private static RouteElementListSummary GetRouteNetworkElementsListSummary(List<IRouteNetworkElement> routeNetworkObjects)
        {
            if (routeNetworkObjects.Count == 0)
                return RouteElementListSummary.None;

            if (routeNetworkObjects.Count(o => o is IRouteNode) == routeNetworkObjects.Count)
                return RouteElementListSummary.RouteNodesOnly;

            if (routeNetworkObjects.Count(o => o is IRouteSegment) == routeNetworkObjects.Count)
                return RouteElementListSummary.RouteSegmentsOnly;

            if (routeNetworkObjects.Count(o => o is IRouteSegment) == routeNetworkObjects.Count)
                return RouteElementListSummary.RouteSegmentsOnly;

            return RouteElementListSummary.BothRouteNodesAndSegments;
        }


        private enum RouteElementListSummary
        {
            None,
            RouteNodesOnly,
            RouteSegmentsOnly,
            BothRouteNodesAndSegments
        }

    }
}
