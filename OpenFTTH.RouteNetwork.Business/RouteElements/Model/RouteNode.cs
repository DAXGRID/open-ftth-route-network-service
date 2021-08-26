using DAX.ObjectVersioning.Graph;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
using System;

namespace OpenFTTH.RouteNetwork.Business.RouteElements.Model
{
    public class RouteNode : GraphNode, IRouteNode
    {
        public string Coordinates { get; set; }
        public RouteNodeInfo? RouteNodeInfo { get; set; }
        public NamingInfo? NamingInfo { get; set; }
        public LifecycleInfo? LifecycleInfo { get; set; }
        public SafetyInfo? SafetyInfo { get; set; }
        public MappingInfo? MappingInfo { get; set; }

        public RouteNode(Guid id, string coordinates) : base(id)
        {
            this.Coordinates = coordinates;
        }
    }
}
