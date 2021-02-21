using OpenFTTH.CQRS;
using OpenFTTH.RouteNetwork.API.Commands;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.API.Queries;
using OpenFTTH.RouteNetwork.Tests.Fixtures;
using System;
using Xunit;
using FluentAssertions;
using FluentResults;
using System.Linq;

namespace OpenFTTH.RouteNetworkService.Tests.Interest
{
    public class InterestCreationTests
    {
        private ICommandDispatcher _commandDispatcher;
        private IQueryDispatcher _queryDispatcher;

        public InterestCreationTests(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
        {
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
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
            Result registerWalkOfInterestCommandResult = await _commandDispatcher.HandleAsync<RegisterWalkOfInterest, Result>(registerWalkOfInterestCommand);

            var routeNetworkQuery = new GetRouteNetworkDetails(new RouteNetworkElementIdList() { TestRouteNetwork.CO_1 })
            {
                RelatedInterestFilter = RelatedInterestFilterOptions.ReferencesFromRouteElementAndInterestObjects
            };

            Result<GetRouteNetworkDetailsResult> routeNetworkQueryResult = await _queryDispatcher.HandleAsync<GetRouteNetworkDetails, Result<GetRouteNetworkDetailsResult>> (routeNetworkQuery);

            // Assert            
            registerWalkOfInterestCommandResult.IsSuccess.Should().BeTrue();
            routeNetworkQueryResult.IsSuccess.Should().BeTrue();

            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Count.Should().Be(3);
            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Should().Contain(TestRouteNetwork.CO_1);
            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Should().Contain(TestRouteNetwork.S1);
            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Should().Contain(TestRouteNetwork.HH_1);

            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Should().NotContain(TestRouteNetwork.S2);
        }

        [Fact]
        public async void CreateValidWalkOfInterestUsingThreeSegments_ShouldReturnSuccess()
        {
            // Route network subset used in this test:
            // (CO_1) <- (S1) -> (HH_1) <- (S2) -> (HH_2) <- (S4) -> (CC_1)
            var interestId = Guid.NewGuid();

            var walk = new RouteNetworkElementIdList() { TestRouteNetwork.S1, TestRouteNetwork.S2, TestRouteNetwork.S4 };

            // Act
            var registerWalkOfInterestCommand = new RegisterWalkOfInterest(interestId, walk);
            Result registerWalkOfInterestCommandResult = await _commandDispatcher.HandleAsync<RegisterWalkOfInterest, Result>(registerWalkOfInterestCommand);

            var routeNetworkQuery = new GetRouteNetworkDetails(new RouteNetworkElementIdList() { TestRouteNetwork.CO_1 })
            {
                RelatedInterestFilter = RelatedInterestFilterOptions.ReferencesFromRouteElementAndInterestObjects
            };

            Result<GetRouteNetworkDetailsResult> routeNetworkQueryResult = await _queryDispatcher.HandleAsync<GetRouteNetworkDetails, Result<GetRouteNetworkDetailsResult>>(routeNetworkQuery);

            // Assert
            registerWalkOfInterestCommandResult.IsSuccess.Should().BeTrue();
            routeNetworkQueryResult.IsSuccess.Should().BeTrue();

            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Count.Should().Be(7);
            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Should().Contain(TestRouteNetwork.CO_1);
            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Should().Contain(TestRouteNetwork.S1);
            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Should().Contain(TestRouteNetwork.HH_1);
            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Should().Contain(TestRouteNetwork.S2);
            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Should().Contain(TestRouteNetwork.HH_2);
            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Should().Contain(TestRouteNetwork.S4);
            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Should().Contain(TestRouteNetwork.CC_1);
        }

        [Fact]
        public async void CreateValidWalkOfInterestOverlappingSegments_ShouldReturnSuccess()
        {
            // Route network subset used in this test:
            // (CO_1) <- (S1) -> (HH_1) <- (S2) -> (HH_2) <- (S1) -> (CO_1)
            var interestId = Guid.NewGuid();

            var walk = new RouteNetworkElementIdList() { TestRouteNetwork.S1, TestRouteNetwork.S2, TestRouteNetwork.S1 };

            // Act
            var registerWalkOfInterestCommand = new RegisterWalkOfInterest(interestId, walk);
            Result registerWalkOfInterestCommandResult = await _commandDispatcher.HandleAsync<RegisterWalkOfInterest, Result>(registerWalkOfInterestCommand);

            var routeNetworkQuery = new GetRouteNetworkDetails(new RouteNetworkElementIdList() { TestRouteNetwork.CO_1 })
            {
                RelatedInterestFilter = RelatedInterestFilterOptions.ReferencesFromRouteElementAndInterestObjects
            };

            Result<GetRouteNetworkDetailsResult> routeNetworkQueryResult = await _queryDispatcher.HandleAsync<GetRouteNetworkDetails, Result<GetRouteNetworkDetailsResult>>(routeNetworkQuery);

            // Assert
            registerWalkOfInterestCommandResult.IsSuccess.Should().BeTrue();
            routeNetworkQueryResult.IsSuccess.Should().BeTrue();

            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Count.Should().Be(7);
            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Should().Contain(TestRouteNetwork.CO_1);
            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Should().Contain(TestRouteNetwork.S1);
            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Should().Contain(TestRouteNetwork.HH_1);
            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Should().Contain(TestRouteNetwork.S2);
            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Should().Contain(TestRouteNetwork.HH_2);
            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Should().Contain(TestRouteNetwork.S1);
            routeNetworkQueryResult.Value.Interests[interestId].RouteNetworkElementRefs.Should().Contain(TestRouteNetwork.CO_1);
        }

        [Fact]
        public async void CreateInvalidWalkOfInterestUsingOneNodeAndOneSegments_ShouldReturnFaliour()
        {
            // Route network subset used in this test:
            // (CO_1) <-> (S1)
            var interestId = Guid.NewGuid();

            var walk = new RouteNetworkElementIdList() { TestRouteNetwork.CO_1, TestRouteNetwork.S1 };

            // Act
            var registerWalkOfInterestCommand = new RegisterWalkOfInterest(interestId, walk);
            Result registerWalkOfInterestCommandResult = await _commandDispatcher.HandleAsync<RegisterWalkOfInterest, Result>(registerWalkOfInterestCommand);

            // Assert
            registerWalkOfInterestCommandResult.IsFailed.Should().BeTrue();
            registerWalkOfInterestCommandResult.Errors.OfType<RegisterWalkOfInterestError>().Should().Contain(e => e.Code == RegisterWalkOfInterestErrorCodes.INVALID_WALK_SHOULD_CONTAIN_ROUTE_SEGMENT_IDS_ONLY);
        }

        [Fact]
        public async void CreateInvalidWalkOfInterestUsingTwoSeparatedSegments_ShouldReturnFaliour()
        {
            // Route network subset used in this test:
            // (CO_1) <- (S1) -> (HH_1) hole in the walk here (HH_2) -> (S4) -> (CC_1)
            var interestId = Guid.NewGuid();

            var walk = new RouteNetworkElementIdList() { TestRouteNetwork.S1, TestRouteNetwork.S4 };

            // Act
            var registerWalkOfInterestCommand = new RegisterWalkOfInterest(interestId, walk);
            Result registerWalkOfInterestCommandResult = await _commandDispatcher.HandleAsync<RegisterWalkOfInterest, Result>(registerWalkOfInterestCommand);

            // Assert
            registerWalkOfInterestCommandResult.IsFailed.Should().BeTrue();
            registerWalkOfInterestCommandResult.Errors.OfType<RegisterWalkOfInterestError>().Should().Contain(e => e.Code == RegisterWalkOfInterestErrorCodes.INVALID_WALK_SEGMENTS_ARE_NOT_ADJACENT);
        }

        [Fact]
        public async void CreateInvalidWalkOfInterestWithNonExistingRouteNetworkElement_ShouldReturnFaliour()
        {
            var interestId = Guid.NewGuid();

            var walk = new RouteNetworkElementIdList() { TestRouteNetwork.S1, TestRouteNetwork.S2, Guid.NewGuid() };

            // Act
            var registerWalkOfInterestCommand = new RegisterWalkOfInterest(interestId, walk);
            Result registerWalkOfInterestCommandResult = await _commandDispatcher.HandleAsync<RegisterWalkOfInterest, Result>(registerWalkOfInterestCommand);

            // Assert
            registerWalkOfInterestCommandResult.IsFailed.Should().BeTrue();
            registerWalkOfInterestCommandResult.Errors.OfType<RegisterWalkOfInterestError>().Should().Contain(e => e.Code == RegisterWalkOfInterestErrorCodes.INVALID_WALK_CANNOT_FIND_ROUTE_NETWORK_ELEMENT);
        }
    }
}
