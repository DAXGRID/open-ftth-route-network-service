using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetwork.API.Queries
{
    public class RouteSegmentQueryResult
    {
        public Guid RouteSegmentId { get; set; }
        public RouteSegmentInfo? RouteSegmentInfo { get; set; }
        public NamingInfo? NamingInfo { get; set; }
        public LifecycleInfo? LifecycleInfo { get; set; }
        public SafetyInfo? SafetyInfo { get; set; }
        public MappingInfo? MappingInfo { get; set; }
        public List<Guid>? WalkOfInterestIds { get; set; }
    }
}
