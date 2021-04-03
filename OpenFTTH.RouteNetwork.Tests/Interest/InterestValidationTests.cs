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

namespace OpenFTTH.RouteNetwork.Tests
{
    public class InterestValidationTests : IClassFixture<TestRouteNetwork>
    {
        private ICommandDispatcher _commandDispatcher;
        private IQueryDispatcher _queryDispatcher;

        public InterestValidationTests(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
        {
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
        }

        [Fact]
        public async void ValidateValidWalk_ShouldSucceed()
        {
            var walk = new RouteNetworkElementIdList() { TestRouteNetwork.S2, TestRouteNetwork.S1, TestRouteNetwork.S4 };
            var validateInterestCommand = new ValidateWalkOfInterest(walk);
            
            var validateResult = await _commandDispatcher.HandleAsync<ValidateWalkOfInterest, Result<ValidatedRouteNetworkWalk>>(validateInterestCommand);

            // Assert
            validateResult.IsSuccess.Should().BeTrue();

            validateResult.Value.NodeIds.Count.Should().Be(4);
            validateResult.Value.SegmentIds.Count.Should().Be(3);

            validateResult.Value.NodeIds[0].Should().Be(TestRouteNetwork.CO_1);
            validateResult.Value.NodeIds[1].Should().Be(TestRouteNetwork.HH_1);
            validateResult.Value.NodeIds[2].Should().Be(TestRouteNetwork.HH_2);
            validateResult.Value.NodeIds[3].Should().Be(TestRouteNetwork.CC_1);

            validateResult.Value.SegmentIds[0].Should().Be(TestRouteNetwork.S1);
            validateResult.Value.SegmentIds[1].Should().Be(TestRouteNetwork.S2);
            validateResult.Value.SegmentIds[2].Should().Be(TestRouteNetwork.S4);
        }

        [Fact]
        public async void ValidateInvalidWalk_ShouldFail()
        {
            // There's a hole in this walk
            var walk = new RouteNetworkElementIdList() { TestRouteNetwork.S2, TestRouteNetwork.S1, TestRouteNetwork.S5 };
            var validateInterestCommand = new ValidateWalkOfInterest(walk);

            var validateResult = await _commandDispatcher.HandleAsync<ValidateWalkOfInterest, Result<ValidatedRouteNetworkWalk>>(validateInterestCommand);

            // Assert
            validateResult.IsFailed.Should().BeTrue();
        }

    }
}
