using DAX.ObjectVersioning.Graph;
using NetTopologySuite.Geometries;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
using OpenFTTH.RouteNetwork.API.Queries;
using System;

namespace OpenFTTH.RouteNetwork.Service.Business.DomainModel.RouteNetwork
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
