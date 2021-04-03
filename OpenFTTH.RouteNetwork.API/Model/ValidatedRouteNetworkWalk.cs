using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenFTTH.RouteNetwork.API.Model
{
    public class ValidatedRouteNetworkWalk
    {
        public RouteNetworkElementIdList RouteNetworkElementRefs { get; }

        public ValidatedRouteNetworkWalk(RouteNetworkElementIdList routeNetworkElementRefs)
        {
            RouteNetworkElementRefs = routeNetworkElementRefs;
        }

        public List<Guid> SegmentIds
        {
            get
            {
                List<Guid> result = new();

                for (int i = 1; i < RouteNetworkElementRefs.Count; i += 2)
                {
                    result.Add(RouteNetworkElementRefs[i]);
                }

                return result;
            }
        }

        public List<Guid> NodeIds
        {
            get
            {
                List<Guid> result = new();

                for (int i = 0; i < RouteNetworkElementRefs.Count; i += 2)
                {
                    result.Add(RouteNetworkElementRefs[i]);
                }

                return result;
            }
        }

        public Guid FromNodeId
        {
            get
            {
                return (RouteNetworkElementRefs.First());
            }
        }

        public Guid ToNodeId
        {
            get
            {
                return (RouteNetworkElementRefs.Last());
            }
        }

    }
}
