using DAX.ObjectVersioning.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFTTH.RouteNetwork.Business.StateHandling
{
    public interface IRouteNetworkState
    {
        ITransaction GetTransaction();
        void FinishWithTransaction();
        IVersionedObject? GetObject(Guid id);
    }
}
