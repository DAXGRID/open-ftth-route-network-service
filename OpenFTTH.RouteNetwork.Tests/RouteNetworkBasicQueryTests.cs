using CSharpFunctionalExtensions;
using OpenFTTH.RouteNetwork.API.Queries;
using OpenFTTH.RouteNetwork.Tests.Fixtures;
using System;
using Xunit;

namespace OpenFTTH.RouteNetwork.Tests
{
    public class RouteNetworkBasicQueryTests : IClassFixture<KongefoldenTestRouteNetwork>
    {
        readonly KongefoldenTestRouteNetwork testNetwork;

        public RouteNetworkBasicQueryTests(KongefoldenTestRouteNetwork testNetwork)
        {
            this.testNetwork = testNetwork;
        }

        [Fact]
        public async void Query_RouteNode_ThatDontExists_ShouldReturnFailure()
        {
            var routeNodeQuery = new RouteNodeQuery(Guid.NewGuid());

            Result<RouteNodeQueryResult> routeNodeQueryResult = await testNetwork.QueryApi.HandleAsync(routeNodeQuery);

            // Assert that the result is marked as a faliure
            Assert.True(routeNodeQueryResult.IsFailure);

            // Assert that the error msg contains the id of the failed lookup
            Assert.Contains(routeNodeQuery.RouteNodeId.ToString(), routeNodeQueryResult.Error);
        }

        [Fact]
        public async void Query_RouteNode_ThatExists_ShouldReturnSuccess()
        {
            // Query some route node that is part of the test network
            var routeNodeQuery = new RouteNodeQuery(Guid.Parse("dab2aea2-873c-4c85-8d33-5907f69437fe"));

            Result <RouteNodeQueryResult> routeNodeQueryResult = await testNetwork.QueryApi.HandleAsync(routeNodeQuery);

            // Assert that the result is marked as success
            Assert.True(routeNodeQueryResult.IsSuccess);

        }

        [Fact]
        public async void Query_RouteNode_WithRouteSegmentId_ShouldReturnFailure()
        {
            // Let's try specify a route segment id in a route node query - should return failure
            var routeNodeQuery = new RouteNodeQuery(Guid.Parse("7035f8f6-e965-4d2d-b205-ee4ace7b3485"));

            Result<RouteNodeQueryResult> routeNodeQueryResult = await testNetwork.QueryApi.HandleAsync(routeNodeQuery);

            // Assert that the result is marked as a faliure
            Assert.True(routeNodeQueryResult.IsFailure);

            // Assert that the error msg contains the id of the failed lookup
            Assert.Contains(routeNodeQuery.RouteNodeId.ToString(), routeNodeQueryResult.Error);
        }
    }
}
