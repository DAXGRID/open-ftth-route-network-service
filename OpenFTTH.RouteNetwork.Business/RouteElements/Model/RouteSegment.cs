using DAX.ObjectVersioning.Graph;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
using System;

namespace OpenFTTH.RouteNetwork.Business.RouteElements.Model
{
    public class RouteSegment : GraphEdge, IRouteSegment
    {
        public string Coordinates { get; set; }
        public RouteSegmentInfo? RouteSegmentInfo { get; set; }
        public NamingInfo? NamingInfo { get; set; }
        public LifecycleInfo? LifecycleInfo { get; set; }
        public SafetyInfo? SafetyInfo { get; set; }
        public MappingInfo? MappingInfo { get; set; }

        public RouteSegment(Guid id, string coordinates, RouteNode fromNode, RouteNode toNode) : base(id, fromNode, toNode)
        {
            this.Coordinates = coordinates;
        }
    }
}
