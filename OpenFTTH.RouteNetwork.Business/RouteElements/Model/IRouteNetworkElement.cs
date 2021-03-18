using OpenFTTH.Events.Core.Infos;
using System;

namespace OpenFTTH.RouteNetwork.Business.RouteElements.Model
{
    public interface IRouteNetworkElement
    {
        Guid Id { get; }
        string Coordinates { get; }
        public NamingInfo? NamingInfo { get; set; }
        public LifecycleInfo? LifecycleInfo { get; set; }
        public SafetyInfo? SafetyInfo { get; set; }
        public MappingInfo? MappingInfo { get; set; }
    }
}
