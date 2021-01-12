using NetTopologySuite.Geometries;
using OpenFTTH.Events.Core.Infos;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFTTH.RouteNetwork.Service.Business.DomainModel.RouteNetwork
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
