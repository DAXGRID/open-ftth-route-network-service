using DAX.ObjectVersioning.Core;
using OpenFTTH.RouteNetwork.Service.Business.DomainModel.RouteNetwork;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFTTH.RouteNetwork.Business.StateHandling
{
    public interface IRouteNetworkState
    {
        ITransaction GetTransaction();
        void FinishWithTransaction();
        long GetLatestCommitedVersion();
        IRouteNetworkElement? GetRouteNetworkElement(Guid id);
        IRouteNetworkElement? GetRouteNetworkElement(Guid id, long versionId);
        void Seed(string routeNetworkEventsJson);
    }
}
