using CSharpFunctionalExtensions;
using OpenFTTH.RouteNetwork.API.Model;
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
            var existingRouteNodeId = Guid.Parse("dab2aea2-873c-4c85-8d33-5907f69437fe"); // Some route node that exists in the test network

            var routeNodeQuery = new GetRouteNetworkDetailsQuery(new RouteNetworkElementIdList() { existingRouteNodeId });

            // Act
            Result<GetRouteNetworkDetailsQueryResult> routeNodeQueryResult = await testNetwork.QueryApi.HandleAsync(routeNodeQuery);

            // Assert
            Assert.True(routeNodeQueryResult.IsSuccess);
            Assert.Single(routeNodeQueryResult.Value.ElementsInfos);

            var theRouteNodeObjectReturned = routeNodeQueryResult.Value.ElementsInfos[0];

            Assert.Equal(existingRouteNodeId, theRouteNodeObjectReturned.Id);
            Assert.Equal(RouteNetworkElementKindEnum.RouteNode, theRouteNodeObjectReturned.Kind);
        }
    }
}
