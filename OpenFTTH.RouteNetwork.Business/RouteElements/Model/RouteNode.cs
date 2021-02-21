using DAX.ObjectVersioning.Graph;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
using System;

namespace OpenFTTH.RouteNetwork.Business.RouteElements.Model
{
    public class RouteNode : GraphNode, IRouteNode
    {
        public string Coordinates { get; }
        public RouteNodeInfo? RouteNodeInfo { get; init; }
        public NamingInfo? NamingInfo { get; init; }
        public LifecycleInfo? LifecycleInfo { get; init; }
        public SafetyInfo? SafetyInfo { get; init; }
        public MappingInfo? MappingInfo { get; init; }

        public RouteNode(Guid id, string coordinates) : base(id)
        {
            this.Coordinates = coordinates;
        }
    }
}
