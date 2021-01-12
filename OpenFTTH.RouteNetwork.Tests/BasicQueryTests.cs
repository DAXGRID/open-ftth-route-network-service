using CSharpFunctionalExtensions;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
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
        public async void QueryRouteElement_ThatExists_ShouldReturnSuccessAndAllRouteElementProperties()
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

            Assert.NotNull(theRouteNodeObjectReturned.Coordinates);
            Assert.NotNull(theRouteNodeObjectReturned.RouteNodeInfo);
            Assert.NotNull(theRouteNodeObjectReturned.NamingInfo);
            Assert.NotNull(theRouteNodeObjectReturned.MappingInfo);
            Assert.NotNull(theRouteNodeObjectReturned.LifecycleInfo);
            Assert.NotNull(theRouteNodeObjectReturned.SafetyInfo);
        }

        [Fact]
        public async void QueryMultiRouteElement_ShouldReturnSuccess()
        {
            // Setup
            var routeNodeQuery = new GetRouteNetworkDetailsQuery(new RouteNetworkElementIdList() { testNetwork.CO_1, testNetwork.S13, testNetwork.S5 });

            // Act
            Result<GetRouteNetworkDetailsQueryResult> queryResult = await testNetwork.QueryApi.HandleAsync(routeNodeQuery);

            // Assert
            Assert.True(queryResult.IsSuccess);
            Assert.Equal(3, queryResult.Value.RouteNetworkElements.Length);
                        
            Assert.Equal(testNetwork.CO_1, queryResult.Value.RouteNetworkElements[0].Id);
            Assert.Equal(RouteNetworkElementKindEnum.RouteNode, queryResult.Value.RouteNetworkElements[0].Kind);

            Assert.Equal(testNetwork.S13, queryResult.Value.RouteNetworkElements[1].Id);
            Assert.Equal(RouteNetworkElementKindEnum.RouteSegment, queryResult.Value.RouteNetworkElements[1].Kind);

            Assert.Equal(testNetwork.S5, queryResult.Value.RouteNetworkElements[2].Id);
            Assert.Equal(RouteNetworkElementKindEnum.RouteSegment, queryResult.Value.RouteNetworkElements[2].Kind);
        }

        [Fact]
        public async void ExplicitlyQueryCoordinatesOnly_ShouldReturnCoordinatesOnly()
        {
            // Setup
            var routeNodeQuery = new GetRouteNetworkDetailsQuery(new RouteNetworkElementIdList() { testNetwork.CO_1, testNetwork.S13 })
            {
                RouteNetworkElementFilter = new RouteNetworkElementFilterOptions()
                {
                    IncludeCoordinates = true
                },
                RelatedInterestFilter = RelatedInterestFilterOptions.None
            };

            // Act
            Result<GetRouteNetworkDetailsQueryResult> queryResult = await testNetwork.QueryApi.HandleAsync(routeNodeQuery);

            // Assert
            Assert.True(queryResult.IsSuccess);
            Assert.Equal(2, queryResult.Value.RouteNetworkElements.Length);

            // CO_1
            Assert.Equal("[559485.6702553608,6209040.000026836]", queryResult.Value.RouteNetworkElements[0].Coordinates);
            Assert.Null(queryResult.Value.RouteNetworkElements[0].RouteNodeInfo);
            Assert.Null(queryResult.Value.RouteNetworkElements[0].RouteSegmentInfo);
            Assert.Null(queryResult.Value.RouteNetworkElements[0].NamingInfo);
            Assert.Null(queryResult.Value.RouteNetworkElements[0].MappingInfo);
            Assert.Null(queryResult.Value.RouteNetworkElements[0].LifecycleInfo);
            Assert.Null(queryResult.Value.RouteNetworkElements[0].SafetyInfo);

            // S13
            Assert.Equal("[[559537.3506715331,6209028.300262455],[559602.7453810525,6209027.060552321]]", queryResult.Value.RouteNetworkElements[1].Coordinates);
            Assert.Null(queryResult.Value.RouteNetworkElements[1].RouteNodeInfo);
            Assert.Null(queryResult.Value.RouteNetworkElements[1].RouteSegmentInfo);
            Assert.Null(queryResult.Value.RouteNetworkElements[1].NamingInfo);
            Assert.Null(queryResult.Value.RouteNetworkElements[1].MappingInfo);
            Assert.Null(queryResult.Value.RouteNetworkElements[1].LifecycleInfo);
            Assert.Null(queryResult.Value.RouteNetworkElements[1].SafetyInfo);
        }

        [Fact]
        public async void ExplicitlyQueryRouteNodeInfoOnly_ShouldReturnRouteNodeInfoOnly()
        {
            // Setup
            var routeNodeQuery = new GetRouteNetworkDetailsQuery(new RouteNetworkElementIdList() { testNetwork.CO_1 })
            {
                RouteNetworkElementFilter = new RouteNetworkElementFilterOptions()
                {
                    IncludeRouteNodeInfo = true
                },
                RelatedInterestFilter = RelatedInterestFilterOptions.None
            };

            // Act
            Result<GetRouteNetworkDetailsQueryResult> queryResult = await testNetwork.QueryApi.HandleAsync(routeNodeQuery);

            // Assert
            Assert.True(queryResult.IsSuccess);
            Assert.Single(queryResult.Value.RouteNetworkElements);
            
            var nodeFromQueryResult = queryResult.Value.RouteNetworkElements[0];

            // Assert that route node info is returned
            Assert.NotNull(nodeFromQueryResult.RouteNodeInfo);
            Assert.Equal(RouteNodeKindEnum.CentralOfficeSmall, nodeFromQueryResult.RouteNodeInfo.Kind);
            Assert.Equal(RouteNodeFunctionEnum.SecondaryNode, nodeFromQueryResult.RouteNodeInfo.Function);

            // Assert that the rest of the information is not set
            Assert.Null(nodeFromQueryResult.Coordinates);
            Assert.Null(nodeFromQueryResult.RouteSegmentInfo);
            Assert.Null(nodeFromQueryResult.NamingInfo);
            Assert.Null(nodeFromQueryResult.MappingInfo);
            Assert.Null(nodeFromQueryResult.LifecycleInfo);
            Assert.Null(nodeFromQueryResult.SafetyInfo);
        }

        [Fact]
        public async void ExplicitlyQueryRouteSegmentInfoOnly_ShouldReturnRouteSegmentInfoOnly()
        {
            // Setup
            var routeNodeQuery = new GetRouteNetworkDetailsQuery(new RouteNetworkElementIdList() { testNetwork.S1 })
            {
                RouteNetworkElementFilter = new RouteNetworkElementFilterOptions()
                {
                    IncludeRouteSegmentInfo = true
                },
                RelatedInterestFilter = RelatedInterestFilterOptions.None
            };

            // Act
            Result<GetRouteNetworkDetailsQueryResult> queryResult = await testNetwork.QueryApi.HandleAsync(routeNodeQuery);

            // Assert
            Assert.True(queryResult.IsSuccess);
            Assert.Single(queryResult.Value.RouteNetworkElements);

            var nodeFromQueryResult = queryResult.Value.RouteNetworkElements[0];

            // Assert that route node info is returned
            Assert.NotNull(nodeFromQueryResult.RouteSegmentInfo);
            Assert.Equal(RouteSegmentKindEnum.Underground, nodeFromQueryResult.RouteSegmentInfo.Kind);

            // Assert that the rest of the information is not set
            Assert.Null(nodeFromQueryResult.Coordinates);
            Assert.Null(nodeFromQueryResult.RouteNodeInfo);
            Assert.Null(nodeFromQueryResult.NamingInfo);
            Assert.Null(nodeFromQueryResult.MappingInfo);
            Assert.Null(nodeFromQueryResult.LifecycleInfo);
            Assert.Null(nodeFromQueryResult.SafetyInfo);
        }

        [Fact]
        public async void ExplicitlyQueryNamingInfoOnly_ShouldReturnNamingInfoOnly()
        {
            // Setup
            var routeNodeQuery = new GetRouteNetworkDetailsQuery(new RouteNetworkElementIdList() { testNetwork.CO_1 })
            {
                RouteNetworkElementFilter = new RouteNetworkElementFilterOptions()
                {
                    IncludeNamingInfo = true
                },
                RelatedInterestFilter = RelatedInterestFilterOptions.None
            };

            // Act
            Result<GetRouteNetworkDetailsQueryResult> queryResult = await testNetwork.QueryApi.HandleAsync(routeNodeQuery);

            // Assert
            Assert.True(queryResult.IsSuccess);
            Assert.Single(queryResult.Value.RouteNetworkElements);

            var nodeFromQueryResult = queryResult.Value.RouteNetworkElements[0];

            // Assert that route node info is returned
            Assert.NotNull(nodeFromQueryResult.NamingInfo);
            Assert.Equal("CO-1", nodeFromQueryResult.NamingInfo.Name);
            Assert.Equal("Central Office 1", nodeFromQueryResult.NamingInfo.Description);

            // Assert that the rest of the information is not set
            Assert.Null(nodeFromQueryResult.Coordinates);
            Assert.Null(nodeFromQueryResult.RouteNodeInfo);
            Assert.Null(nodeFromQueryResult.RouteSegmentInfo);
            Assert.Null(nodeFromQueryResult.MappingInfo);
            Assert.Null(nodeFromQueryResult.LifecycleInfo);
            Assert.Null(nodeFromQueryResult.SafetyInfo);
        }

        [Fact]
        public async void ExplicitlyQueryMappingInfoOnly_ShouldReturnMappingInfoOnly()
        {
            // Setup
            var routeNodeQuery = new GetRouteNetworkDetailsQuery(new RouteNetworkElementIdList() { testNetwork.CO_1 })
            {
                RouteNetworkElementFilter = new RouteNetworkElementFilterOptions()
                {
                    IncludeMappingInfo = true
                },
                RelatedInterestFilter = RelatedInterestFilterOptions.None
            };

            // Act
            Result<GetRouteNetworkDetailsQueryResult> queryResult = await testNetwork.QueryApi.HandleAsync(routeNodeQuery);

            // Assert
            Assert.True(queryResult.IsSuccess);
            Assert.Single(queryResult.Value.RouteNetworkElements);

            var nodeFromQueryResult = queryResult.Value.RouteNetworkElements[0];

            // Assert that route node info is returned
            Assert.NotNull(nodeFromQueryResult.MappingInfo);
            Assert.Equal(MappingMethodEnum.Schematic, nodeFromQueryResult.MappingInfo.Method);

            // Assert that the rest of the information is not set
            Assert.Null(nodeFromQueryResult.Coordinates);
            Assert.Null(nodeFromQueryResult.RouteNodeInfo);
            Assert.Null(nodeFromQueryResult.RouteSegmentInfo);
            Assert.Null(nodeFromQueryResult.NamingInfo);
            Assert.Null(nodeFromQueryResult.LifecycleInfo);
            Assert.Null(nodeFromQueryResult.SafetyInfo);
        }

        [Fact]
        public async void ExplicitlyQueryLifecyleInfoOnly_ShouldReturnLifecyleInfoOnly()
        {
            // Setup
            var routeNodeQuery = new GetRouteNetworkDetailsQuery(new RouteNetworkElementIdList() { testNetwork.CO_1 })
            {
                RouteNetworkElementFilter = new RouteNetworkElementFilterOptions()
                {
                    IncludeLifecycleInfo = true
                },
                RelatedInterestFilter = RelatedInterestFilterOptions.None
            };

            // Act
            Result<GetRouteNetworkDetailsQueryResult> queryResult = await testNetwork.QueryApi.HandleAsync(routeNodeQuery);

            // Assert
            Assert.True(queryResult.IsSuccess);
            Assert.Single(queryResult.Value.RouteNetworkElements);

            var nodeFromQueryResult = queryResult.Value.RouteNetworkElements[0];

            // Assert that route node info is returned
            Assert.NotNull(nodeFromQueryResult.LifecycleInfo);
            Assert.Equal(DeploymentStateEnum.InService, nodeFromQueryResult.LifecycleInfo.DeploymentState);

            // Assert that the rest of the information is not set
            Assert.Null(nodeFromQueryResult.Coordinates);
            Assert.Null(nodeFromQueryResult.RouteNodeInfo);
            Assert.Null(nodeFromQueryResult.RouteSegmentInfo);
            Assert.Null(nodeFromQueryResult.NamingInfo);
            Assert.Null(nodeFromQueryResult.MappingInfo);
            Assert.Null(nodeFromQueryResult.SafetyInfo);
        }

        [Fact]
        public async void ExplicitlyQuerySaftyInfoOnly_ShouldReturnSaftyInfoOnly()
        {
            // Setup
            var routeNodeQuery = new GetRouteNetworkDetailsQuery(new RouteNetworkElementIdList() { testNetwork.CO_1 })
            {
                RouteNetworkElementFilter = new RouteNetworkElementFilterOptions()
                {
                    IncludeSafetyInfo = true
                },
                RelatedInterestFilter = RelatedInterestFilterOptions.None
            };

            // Act
            Result<GetRouteNetworkDetailsQueryResult> queryResult = await testNetwork.QueryApi.HandleAsync(routeNodeQuery);

            // Assert
            Assert.True(queryResult.IsSuccess);
            Assert.Single(queryResult.Value.RouteNetworkElements);

            var nodeFromQueryResult = queryResult.Value.RouteNetworkElements[0];

            // Assert that route node info is returned
            Assert.NotNull(nodeFromQueryResult.SafetyInfo);
            Assert.Equal("Ikke farlig", nodeFromQueryResult.SafetyInfo.Classification);

            // Assert that the rest of the information is not set
            Assert.Null(nodeFromQueryResult.Coordinates);
            Assert.Null(nodeFromQueryResult.RouteNodeInfo);
            Assert.Null(nodeFromQueryResult.RouteSegmentInfo);
            Assert.Null(nodeFromQueryResult.NamingInfo);
            Assert.Null(nodeFromQueryResult.MappingInfo);
            Assert.Null(nodeFromQueryResult.LifecycleInfo);
        }
    }
}
