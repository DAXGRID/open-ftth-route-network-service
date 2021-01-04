using CSharpFunctionalExtensions;
using OpenFTTH.CQRS;
using OpenFTTH.RouteNetwork.API.Model;
using System;

namespace OpenFTTH.RouteNetwork.API.Queries
{
    /// <summary>
    /// Used to query detailed information of one or more route network elements (route nodes and/or segments),
    /// including related interest information if explicitly requested. The query supports fetching information 
    /// by means of a list of route network ids or interest ids.
    /// </summary>
    public class GetRouteNetworkDetailsQuery : IQuery<Result<GetRouteNetworkDetailsQueryResult>>
    {
        public static string RequestName => typeof(GetRouteNetworkDetailsQuery).Name;

        /// <summary>
        /// List of route network element ids (route nodes and/or route segments) specified by the calling client
        /// </summary>
        public RouteNetworkElementIdList RouteNetworkElementIdsToQuery { get; }

        /// <summary>
        /// List of interest ids (point and/or walk of interests) specified by the calling client
        /// </summary>
        public InterestIdList InterestIdsToQuery { get; }


        /// <summary>
        /// Use this contructor, if you want to query by route network element ids
        /// </summary>
        /// <param name="routeNetworkElementIds"></param>
        public GetRouteNetworkDetailsQuery(RouteNetworkElementIdList routeNetworkElementIds)
        {
            // Add empty list to InterestIdsToQuery, because the client want to query by route network element ids
            InterestIdsToQuery = new InterestIdList(); 

            if (routeNetworkElementIds == null || routeNetworkElementIds.Count == 0)
                throw new ArgumentException("At least one route node or segment id must be specified using the routeNetworkElementIds parameter");

            this.RouteNetworkElementIdsToQuery = routeNetworkElementIds;
        }

        /// <summary>
        /// Use this contructor, if you want to query by interest ids
        /// </summary>
        /// <param name="interestIds"></param>
        public GetRouteNetworkDetailsQuery(InterestIdList interestIds)
        {
            // Add empty list to RouteNetworkElementIdsToQuery, because the client want to query by interest ids
            RouteNetworkElementIdsToQuery = new RouteNetworkElementIdList();

            if (interestIds == null || interestIds.Count == 0)
                throw new ArgumentException("At least one interest id must be specified using the interestIds parameter");

            this.InterestIdsToQuery = interestIds;
        }

    }
}
