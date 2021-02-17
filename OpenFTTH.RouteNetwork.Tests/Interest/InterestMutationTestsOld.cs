using CSharpFunctionalExtensions;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.API.Commands;
using OpenFTTH.RouteNetwork.API.Queries;
using OpenFTTH.RouteNetwork.Tests.Fixtures;
using System;
using Xunit;

namespace OpenFTTH.RouteNetwork.Tests
{
    public class InterestMutationTestsOld : IClassFixture<TestRouteNetwork>
    {
        readonly TestRouteNetwork testNetwork;

        public InterestMutationTestsOld(TestRouteNetwork testNetwork)
        {
            this.testNetwork = testNetwork;
        }

        [Fact]
        public async void CreateValidWalkOfInterestUsingOneSegmentIdOnly_ShouldReturnSuccess()
        {
            // Route network subset used in this test:
            // (CO_1) <- (S1) -> (HH_1)
            var interestId = Guid.NewGuid();

            var walk = new RouteNetworkElementIdList() { TestRouteNetwork.S1 };

            // Act
            var registerWalkOfInterestCommand = new RegisterWalkOfInterest(interestId, walk);

            Result registerWalkOfInterestCommandResult = await testNetwork.CommandApi.HandleAsync(registerWalkOfInterestCommand);

            // Assert command success and that the command result include all three route network element ids
            Assert.True(registerWalkOfInterestCommandResult.IsSuccess);
            /*
            Assert.Equal(3, registerWalkOfInterestCommandResult.Value.Walk.Count);
            Assert.Equal(TestRouteNetwork.CO_1, registerWalkOfInterestCommandResult.Value.Walk[0]);
            Assert.Equal(TestRouteNetwork.S1, registerWalkOfInterestCommandResult.Value.Walk[1]);
            Assert.Equal(TestRouteNetwork.HH_1, registerWalkOfInterestCommandResult.Value.Walk[2]);
            */
        }

        [Fact]
        public async void CreateValidWalkOfInterestUsingThreeSegments_ShouldReturnSuccess()
        {
            // Route network subset used in this test:
            // (CO_1) <- (S1) -> (HH_1) <- (S2) -> (HH_2) <- (S4) -> (CC_1)
            var interestId = Guid.NewGuid();

            var walk = new RouteNetworkElementIdList() { TestRouteNetwork.S1, TestRouteNetwork.S2, TestRouteNetwork.S4 };

            var createInterestCommand = new RegisterWalkOfInterest(interestId, walk);

            // Act
            Result registerWalkOfInterestCommandResult = await testNetwork.CommandApi.HandleAsync(createInterestCommand);

            // Assert command success and that the result include all three route network element ids
            Assert.True(registerWalkOfInterestCommandResult.IsSuccess);
            /*
            Assert.Equal(7, registerWalkOfInterestCommandResult.Value.Walk.Count);
            Assert.Equal(TestRouteNetwork.CO_1, registerWalkOfInterestCommandResult.Value.Walk[0]);
            Assert.Equal(TestRouteNetwork.S1, registerWalkOfInterestCommandResult.Value.Walk[1]);
            Assert.Equal(TestRouteNetwork.HH_1, registerWalkOfInterestCommandResult.Value.Walk[2]);
            Assert.Equal(TestRouteNetwork.S2, registerWalkOfInterestCommandResult.Value.Walk[3]);
            Assert.Equal(TestRouteNetwork.HH_2, registerWalkOfInterestCommandResult.Value.Walk[4]);
            Assert.Equal(TestRouteNetwork.S4, registerWalkOfInterestCommandResult.Value.Walk[5]);
            Assert.Equal(TestRouteNetwork.CC_1, registerWalkOfInterestCommandResult.Value.Walk[6]);
            */
        }

        [Fact]
        public async void CreateValidWalkOfInterestOverlappingSegments_ShouldReturnSuccess()
        {
            // Route network subset used in this test:
            // (CO_1) <- (S1) -> (HH_1) <- (S2) -> (HH_2) <- (S4) -> (CC_1)
            var interestId = Guid.NewGuid();

            var walk = new RouteNetworkElementIdList() { TestRouteNetwork.S1, TestRouteNetwork.S2, TestRouteNetwork.S1 };

            var createInterestCommand = new RegisterWalkOfInterest(interestId, walk);

            // Act
            Result registerWalkOfInterestCommandResult = await testNetwork.CommandApi.HandleAsync(createInterestCommand);

            // Assert command success and that the result include all three route network element ids
            Assert.True(registerWalkOfInterestCommandResult.IsSuccess);
            /*
            Assert.Equal(7, registerWalkOfInterestCommandResult.Value.Walk.Count);
            Assert.Equal(TestRouteNetwork.CO_1, registerWalkOfInterestCommandResult.Value.Walk[0]);
            Assert.Equal(TestRouteNetwork.S1, registerWalkOfInterestCommandResult.Value.Walk[1]);
            Assert.Equal(TestRouteNetwork.HH_1, registerWalkOfInterestCommandResult.Value.Walk[2]);
            Assert.Equal(TestRouteNetwork.S2, registerWalkOfInterestCommandResult.Value.Walk[3]);
            Assert.Equal(TestRouteNetwork.HH_2, registerWalkOfInterestCommandResult.Value.Walk[4]);
            Assert.Equal(TestRouteNetwork.S1, registerWalkOfInterestCommandResult.Value.Walk[5]);
            Assert.Equal(TestRouteNetwork.CO_1, registerWalkOfInterestCommandResult.Value.Walk[6]);
            */
        }

        [Fact]
        public async void CreateInvalidWalkOfInterestUsingOneNodeAndOneSegments_ShouldReturnFaliour()
        {
            // Route network subset used in this test:
            // (CO_1) <- (S1)
            var interestId = Guid.NewGuid();

            var walk = new RouteNetworkElementIdList() { TestRouteNetwork.CO_1, TestRouteNetwork.S1 };

            var createInterestCommand = new RegisterWalkOfInterest(interestId, walk);

            // Act
            Result registerWalkOfInterestCommandResult = await testNetwork.CommandApi.HandleAsync(createInterestCommand);

            // Assert
            Assert.True(registerWalkOfInterestCommandResult.IsFailure);
        }

        [Fact]
        public async void CreateInvalidWalkOfInterestUsingTwoSeparatedSegments_ShouldReturnFaliour()
        {
            // Route network subset used in this test:
            // (CO_1) <- (S1) -> (HH_1) hole in the walk here (HH_2) -> (S4) -> (CC_1)
            var interestId = Guid.NewGuid();

            var walk = new RouteNetworkElementIdList() { TestRouteNetwork.S1, TestRouteNetwork.S4 };

            var createInterestCommand = new RegisterWalkOfInterest(interestId, walk);

            // Act
            Result registerWalkOfInterestCommandResult = await testNetwork.CommandApi.HandleAsync(createInterestCommand);

            // Assert
            Assert.True(registerWalkOfInterestCommandResult.IsFailure);
        }



    }
}
