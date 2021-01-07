using OpenFTTH.RouteNetwork.API.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetwork.Business.DomainModel.Interest
{
    public record Walk
    {
        public RouteNetworkElementIdList WalkIds { get; }

        public Walk(RouteNetworkElementIdList walkIds)
        {
            this.WalkIds = walkIds;
        }
    }
}
