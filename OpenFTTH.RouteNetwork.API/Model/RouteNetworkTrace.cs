using OpenFTTH.Core;
using System;

namespace OpenFTTH.RouteNetwork.API.Model
{
    public record RouteNetworkTrace : IIdentifiedObject
    {
        public Guid DestNodeId { get; }
        public string? Name { get; }
        public double Distance { get; }
        public Guid[] RouteNetworkSegmentIds { get; }
        public string[] RouteNetworkSegmentGeometries { get; }

        public Guid Id => DestNodeId;
        public string? Description => null;

        public RouteNetworkTrace(Guid destNodeId, string? name, double distance, Guid[] routeNetworkSegmentIds, string[] routeNetworkSegmentGeometries)
        {
            DestNodeId = destNodeId;
            Name = name;
            Distance = distance;
            RouteNetworkSegmentIds = routeNetworkSegmentIds;
            RouteNetworkSegmentGeometries = routeNetworkSegmentGeometries;
        }
    }
}
