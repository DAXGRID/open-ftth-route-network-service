using OpenFTTH.RouteNetwork.API.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetwork.API.Commands
{
    public class RegisterWalkOfInterestCommandResult
    {
        public RouteNetworkElementIdList Walk { get; }

        public RegisterWalkOfInterestCommandResult(RouteNetworkElementIdList walkIds)
        {
            this.Walk = walkIds;
        }
    }
}
