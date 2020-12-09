using DAX.ObjectVersioning.Graph;
using NetTopologySuite.Geometries;
using OpenFTTH.Events.RouteNetwork.Infos;
using OpenFTTH.RouteNetworkService.Queries;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFTTH.RouteNetworkService.Business.Model.RouteNetwork
{
    public class RouteNode : GraphNode, IRouteNode
    {
        private readonly RouteNodeFunctionEnum? _function;
        private readonly string? _name;
        private readonly Envelope _envelope;

        public RouteNodeFunctionEnum? Function => _function;
        public string? Name => _name;
        public Envelope Envelope => _envelope;

        public RouteNodeInfo RouteNodeInfo { get; set; }


        public RouteNode(Guid id, RouteNodeFunctionEnum? function, Envelope envelope, string? name = null) : base(id)
        {
            _function = function;
            _envelope = envelope;
            _name = name;
        }

        internal RouteNodeQueryResult GetQueryResult()
        {
            return new RouteNodeQueryResult()
            {
                RouteNodeId = Id,
                RouteNodeInfo = this.RouteNodeInfo
            };
        }
    }
}
