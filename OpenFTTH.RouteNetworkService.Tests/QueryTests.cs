using OpenFTTH.RouteNetworkService.Repositories.InMemoryImpl;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace OpenFTTH.RouteNetworkService.Tests
{
    public class QueryTests
    {
        [Fact]
        public void TestRouteNodeQuery()
        {
            var repo = new InMemRouteNodeRepository();
        }
    }
}
