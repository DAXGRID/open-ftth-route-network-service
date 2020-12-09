using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using OpenFTTH.RouteNetwork.Business.StateHandling.InMemory;
using OpenFTTH.RouteNetworkService.Queries;
using OpenFTTH.RouteNetworkService.QueryHandlers;
using OpenFTTH.RouteNetworkService.Tests.Fixtures;
using System;
using Xunit;

namespace OpenFTTH.UtilityGraphService.Business.Tests
{
    public class RouteNetworkBasicQueryTests : IClassFixture<KongefoldenTestRouteNetwork>
    {
        readonly KongefoldenTestRouteNetwork testNetwork;

        public RouteNetworkBasicQueryTests(KongefoldenTestRouteNetwork testNetwork)
        {
            this.testNetwork = testNetwork;
        }

        [Fact]
        public async void Query_RouteNode_ThatDontExists_ShouldReturnFaliureResult()
        {
            var routeNodeQuery = new RouteNodeQuery(Guid.NewGuid());

            Result<RouteNodeQueryResult> routeNodeQueryResult = await testNetwork.QueryApi.HandleAsync(routeNodeQuery);

            // Assert that the result is marked as a faliure
            Assert.True(routeNodeQueryResult.IsFailure);

            // Assert that the error msg contains the id of the failed lookup
            Assert.Contains(routeNodeQuery.RouteNodeId.ToString(), routeNodeQueryResult.Error);
        }

       

    }
}
