using CSharpFunctionalExtensions;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.API.Commands;
using OpenFTTH.RouteNetwork.API.Queries;
using OpenFTTH.RouteNetwork.Tests.Fixtures;
using System;
using System.Linq;
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
        public async void QueryReferencesFromRouteElementAndInterestObjects_ShouldReturnAllInterestInformation()
        {
            // Create interest (CO_1) <- (S1) -> (HH_1) that we can then try query
            var interestId = Guid.NewGuid();
            var walk = new RouteNetworkElementIdList() { testNetwork.S1 };
            var createInterestCommand = new RegisterWalkOfInterestCommand(interestId, walk);
            
            await testNetwork.CommandApi.HandleAsync(createInterestCommand);

            // Act: Query CO_1, S1, HH_1 and HH_2
            var routeNetworkQuery = new GetRouteNetworkDetailsQuery(new RouteNetworkElementIdList() { testNetwork.CO_1, testNetwork.S1, testNetwork.HH_1, testNetwork.HH_2 })
            {
                RelatedInterestFilter = RelatedInterestFilterOptions.ReferencesFromRouteElementAndInterestObjects
            };
            
            Result<GetRouteNetworkDetailsQueryResult> queryResult = await testNetwork.QueryApi.HandleAsync(routeNetworkQuery);

            // Assert that we got information back on all 4 network elements queried
            Assert.True(queryResult.IsSuccess);
            Assert.Equal(4, queryResult.Value.RouteNetworkElements.Length);

            // Assert that we got back one interest related to the 3 network elements that the walk of interest covers
            Assert.NotNull(queryResult.Value.Interests);
            Assert.Contains(queryResult.Value.Interests, i => i.Id == interestId);
            Assert.Equal(RouteNetworkInterestKindEnum.WalkOfInterest, queryResult.Value.Interests[0].Kind);
            Assert.Equal(3, queryResult.Value.Interests[0].RouteNetworkElementRefs.Count);
            Assert.Contains(testNetwork.CO_1, queryResult.Value.Interests[0].RouteNetworkElementRefs);
            Assert.Contains(testNetwork.S1, queryResult.Value.Interests[0].RouteNetworkElementRefs);
            Assert.Contains(testNetwork.HH_1, queryResult.Value.Interests[0].RouteNetworkElementRefs);

            // Assert that route element 1 (CO_1) has interest information with correct relation type
            Assert.NotNull(queryResult.Value.RouteNetworkElements[0].InterestRelations);
            Assert.Contains(queryResult.Value.RouteNetworkElements[0].InterestRelations, r => r.RefId == interestId && r.RelationKind == RouteNetworkInterestRelationKindEnum.Start);

            // Assert that route element 2 (S1) has interest information with correct relation type
            Assert.NotNull(queryResult.Value.RouteNetworkElements[1].InterestRelations);
            Assert.Contains(queryResult.Value.RouteNetworkElements[1].InterestRelations, r => r.RefId == interestId && r.RelationKind == RouteNetworkInterestRelationKindEnum.PassThrough);

            // Assert that route element 3 (HH_1) has interest information with correct relation type
            Assert.NotNull(queryResult.Value.RouteNetworkElements[2].InterestRelations);
            Assert.Contains(queryResult.Value.RouteNetworkElements[2].InterestRelations, r => r.RefId == interestId && r.RelationKind == RouteNetworkInterestRelationKindEnum.End);

            // Assert that route element 4 (HH_2) has no references to interest information
            Assert.Empty(queryResult.Value.RouteNetworkElements[3].InterestRelations);
        }

        [Fact]
        public async void QueryReferencesFromRouteElementOnly_ShouldReturnInterestReferencesOnly()
        {
            // Create interest (CO_1) <- (S1) -> (HH_1) that we can then try query
            var interestId = Guid.NewGuid();
            var walk = new RouteNetworkElementIdList() { testNetwork.S1 };
            var createInterestCommand = new RegisterWalkOfInterestCommand(interestId, walk);

            await testNetwork.CommandApi.HandleAsync(createInterestCommand);

            // Act: Query CO_1, S1, HH_1 and HH_2
            var routeNetworkQuery = new GetRouteNetworkDetailsQuery(new RouteNetworkElementIdList() { testNetwork.CO_1, testNetwork.S1, testNetwork.HH_1, testNetwork.HH_2 })
            {
                RelatedInterestFilter = RelatedInterestFilterOptions.ReferencesFromRouteElementOnly
            };

            Result<GetRouteNetworkDetailsQueryResult> queryResult = await testNetwork.QueryApi.HandleAsync(routeNetworkQuery);

            // Assert that we got information back on all 4 network elements queried
            Assert.True(queryResult.IsSuccess);
            Assert.Equal(4, queryResult.Value.RouteNetworkElements.Length);

            // Assert that we did'nt get any interest object back
            Assert.Null(queryResult.Value.Interests);

            // Assert that route element 1 (CO_1) has interest information with correct relation type
            Assert.NotNull(queryResult.Value.RouteNetworkElements[0].InterestRelations);
            Assert.Contains(queryResult.Value.RouteNetworkElements[0].InterestRelations, r => r.RefId == interestId && r.RelationKind == RouteNetworkInterestRelationKindEnum.Start);

            // Assert that route element 2 (S1) has interest information with correct relation type
            Assert.NotNull(queryResult.Value.RouteNetworkElements[1].InterestRelations);
            Assert.Contains(queryResult.Value.RouteNetworkElements[1].InterestRelations, r => r.RefId == interestId && r.RelationKind == RouteNetworkInterestRelationKindEnum.PassThrough);

            // Assert that route element 3 (HH_1) has interest information with correct relation type
            Assert.NotNull(queryResult.Value.RouteNetworkElements[2].InterestRelations);
            Assert.Contains(queryResult.Value.RouteNetworkElements[2].InterestRelations, r => r.RefId == interestId && r.RelationKind == RouteNetworkInterestRelationKindEnum.End);

            // Assert that route element 4 (HH_2) has no references to interest information
            Assert.Empty(queryResult.Value.RouteNetworkElements[3].InterestRelations);
        }

        [Fact]
        public async void QueryExplicitlyRequestingNoInterestInformation_ShouldReturnNoInterestInformation()
        {
            // Create interest (CO_1) <- (S1) -> (HH_1) that we can then try query
            var interestId = Guid.NewGuid();
            var walk = new RouteNetworkElementIdList() { testNetwork.S1 };
            var createInterestCommand = new RegisterWalkOfInterestCommand(interestId, walk);

            await testNetwork.CommandApi.HandleAsync(createInterestCommand);

            // Act: Query CO_1, S1, HH_1 and HH_2
            var routeNetworkQuery = new GetRouteNetworkDetailsQuery(new RouteNetworkElementIdList() { testNetwork.CO_1, testNetwork.S1, testNetwork.HH_1, testNetwork.HH_2 })
            {
                RelatedInterestFilter = RelatedInterestFilterOptions.None
            };

            Result<GetRouteNetworkDetailsQueryResult> queryResult = await testNetwork.QueryApi.HandleAsync(routeNetworkQuery);

            // Assert that we got information back on all 4 network elements queried
            Assert.True(queryResult.IsSuccess);
            Assert.Equal(4, queryResult.Value.RouteNetworkElements.Length);

            // Assert that we did'nt get any interest object back
            Assert.Null(queryResult.Value.Interests);

            // Assert that no route elements got interest relations
            Assert.Null(queryResult.Value.RouteNetworkElements[0].InterestRelations);
            Assert.Null(queryResult.Value.RouteNetworkElements[1].InterestRelations);
            Assert.Null(queryResult.Value.RouteNetworkElements[2].InterestRelations);
            Assert.Null(queryResult.Value.RouteNetworkElements[3].InterestRelations);
        }
    }
}
