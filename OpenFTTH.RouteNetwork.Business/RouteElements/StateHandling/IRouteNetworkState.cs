﻿using DAX.ObjectVersioning.Core;
using OpenFTTH.RouteNetwork.Business.RouteElements.Model;
using System;

namespace OpenFTTH.RouteNetwork.Business.RouteElements.StateHandling
{
    public interface IRouteNetworkState
    {
        ITransaction GetTransaction();
        void FinishWithTransaction();
        long GetLatestCommitedVersion();
        IRouteNetworkElement? GetRouteNetworkElement(Guid id);
        IRouteNetworkElement? GetRouteNetworkElement(Guid id, long versionId);
        void Seed(string routeNetworkEventsJson);
        bool IsLoadMode { get; }
    }
}
