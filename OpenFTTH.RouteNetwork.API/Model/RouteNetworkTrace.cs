using OpenFTTH.Core;
using System;

namespace OpenFTTH.RouteNetwork.API.Model
{
    public record RouteNetworkTrace : IIdentifiedObject
    {
        public Guid DestNodeId { get; }
        public double Distance { get; }
        public Guid[] RouteNetworkSegmentIds { get; }

        public Guid Id => DestNodeId;
        public string? Name => null;
        public string? Description => null;

        public RouteNetworkTrace(Guid destNodeId, double distance, Guid[] routeNetworkSegmentIds)
        {
            DestNodeId = destNodeId;
            Distance = distance;
            RouteNetworkSegmentIds = routeNetworkSegmentIds;
        }
    }
}
