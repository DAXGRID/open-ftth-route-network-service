using CSharpFunctionalExtensions;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.API.Queries;
using OpenFTTH.RouteNetwork.Tests.Fixtures;
using System;
using Xunit;

namespace OpenFTTH.RouteNetwork.Tests
{
    public class BasicQueryTests : IClassFixture<TestRouteNetwork>
    {
        readonly TestRouteNetwork testNetwork;

        public BasicQueryTests(TestRouteNetwork testNetwork)
        {
            this.testNetwork = testNetwork;
        }

        [Fact]
        public async void QueryRouteElement_ThatDontExists_ShouldReturnFailure()
        {
            // Setup
            var nonExistingRouteNetworkElementId = Guid.NewGuid();

            var routeNodeQuery = new GetRouteNetworkDetailsQuery(new RouteNetworkElementIdList() { nonExistingRouteNetworkElementId });

            // Act
            Result<GetRouteNetworkDetailsQueryResult> routeNodeQueryResult = await testNetwork.QueryApi.HandleAsync(routeNodeQuery);

            // Assert
            Assert.True(routeNodeQueryResult.IsFailure);

            // Assert that the error msg contains the id of route network element that the service could not lookup
            Assert.Contains(nonExistingRouteNetworkElementId.ToString(), routeNodeQueryResult.Error);
        }
        

        [Fact]
        public async void QueryRouteElement_ThatExists_ShouldReturnSuccess()
        {
            // Setup
            var routeNodeQuery = new GetRouteNetworkDetailsQuery(new RouteNetworkElementIdList() { testNetwork.CO_1 });

            // Act
            Result<GetRouteNetworkDetailsQueryResult> routeNodeQueryResult = await testNetwork.QueryApi.HandleAsync(routeNodeQuery);

            // Assert
            Assert.True(routeNodeQueryResult.IsSuccess);
            Assert.Single(routeNodeQueryResult.Value.RouteNetworkElements);

            var theRouteNodeObjectReturned = routeNodeQueryResult.Value.RouteNetworkElements[0];

            Assert.Equal(testNetwork.CO_1, theRouteNodeObjectReturned.Id);
            Assert.Equal(RouteNetworkElementKindEnum.RouteNode, theRouteNodeObjectReturned.Kind);
        }
    }
}
