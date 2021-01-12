using DAX.ObjectVersioning.Graph;
using NetTopologySuite.Geometries;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFTTH.RouteNetwork.Service.Business.DomainModel.RouteNetwork
{
    public class RouteSegment : GraphEdge, IRouteSegment
    {
        public string Coordinates { get; }
        public RouteSegmentInfo? RouteSegmentInfo { get; init; }
        public NamingInfo? NamingInfo { get; init; }
        public LifecycleInfo? LifecycleInfo { get; init; }
        public SafetyInfo? SafetyInfo { get; init; }
        public MappingInfo? MappingInfo { get; init; }

        public RouteSegment(Guid id, string coordinates, RouteNode fromNode, RouteNode toNode) : base(id, fromNode, toNode)
        {
            this.Coordinates = coordinates;
        }
    }
}
