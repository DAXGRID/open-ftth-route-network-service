using CSharpFunctionalExtensions;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.API.Mutations;
using OpenFTTH.RouteNetwork.API.Queries;
using OpenFTTH.RouteNetwork.Tests.Fixtures;
using System;
using Xunit;

namespace OpenFTTH.RouteNetwork.Tests
{
    public class InterestQueryTests : IClassFixture<TestRouteNetwork>
    {
        readonly TestRouteNetwork testNetwork;

        public InterestQueryTests(TestRouteNetwork testNetwork)
        {
            this.testNetwork = testNetwork;
        }

        [Fact]
        public async void QueryRouteElementsRelatedToOneWalkOfInterest_ShouldReturnSuccess()
        {
            // Create interest (CO_1) <- (S1) -> (HH_1) that we can then query
            var interestId = Guid.NewGuid();
            var walk = new RouteNetworkElementIdList() { testNetwork.S1 };
            var createInterestCommand = new RegisterWalkOfInterestCommand(interestId, walk);
            
            await testNetwork.CommandApi.HandleAsync(createInterestCommand);

            // Query CO_1, S1 and HH_1
            var routeNodeQuery = new GetRouteNetworkDetailsQuery(new RouteNetworkElementIdList() { testNetwork.CO_1, testNetwork.S1, testNetwork.HH_1 })
            {
                IncludeRelatedInterest = true
            };

            // Act
            Result<GetRouteNetworkDetailsQueryResult> routeNodeQueryResult = await testNetwork.QueryApi.HandleAsync(routeNodeQuery);

            // Assert
            Assert.True(routeNodeQueryResult.IsSuccess);

        }
    }
}
