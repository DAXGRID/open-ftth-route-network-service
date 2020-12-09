using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFTTH.RouteNetwork.Service.Business.DomainModel.RouteNetwork
{
    public interface IRouteNetworkElement
    {
        Guid Id { get; }
        Envelope Envelope { get; }
    }
}
