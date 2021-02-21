using OpenFTTH.Events.Core.Infos;
using System;

namespace OpenFTTH.RouteNetwork.Business.RouteElements.Model
{
    public interface IRouteNetworkElement
    {
        Guid Id { get; }
        string Coordinates { get; }
        public NamingInfo? NamingInfo { get; }
        public LifecycleInfo? LifecycleInfo { get; }
        public SafetyInfo? SafetyInfo { get;  }
        public MappingInfo? MappingInfo { get;  }
    }
}
