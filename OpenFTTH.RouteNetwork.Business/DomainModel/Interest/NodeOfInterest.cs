using OpenFTTH.RouteNetwork.API.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetwork.Business.DomainModel.Interest
{
    public class NodeOfInterest : IInterest
    {
        private Guid _routeNodeId;
        public Guid Id { get; }

        public RouteNetworkElementIdList RouteNetworkElementIds
        {
            get
            {
                return new RouteNetworkElementIdList() { _routeNodeId };
            }
        }

        public NodeOfInterest(Guid id, Guid routeNodeId)
        {
            this.Id = id;
            this._routeNodeId = routeNodeId;
        }
    }
}
