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
    public class InterestQueryTestsOld : IClassFixture<TestRouteNetwork>
    {
        readonly TestRouteNetwork testNetwork;

        public InterestQueryTestsOld(TestRouteNetwork testNetwork)
        {
            this.testNetwork = testNetwork;
        }

        [Fact]
        public async void QueryReferencesFromRouteElementAndInterestObjects_ShouldReturnAllInterestInformation()
        {
            // Create interest (CO_1) <- (S1) -> (HH_1) that we can then try query
            var interestId = Guid.NewGuid();
            var walk = new RouteNetworkElementIdList() { TestRouteNetwork.S1 };
            var createInterestCommand = new RegisterWalkOfInterest(interestId, walk);
            
            await testNetwork.CommandApi.HandleAsync(createInterestCommand);

            // Act: Query CO_1, S1, HH_1 and HH_2
            var routeNetworkQuery = new GetRouteNetworkDetails(new RouteNetworkElementIdList() { TestRouteNetwork.CO_1, TestRouteNetwork.S1, TestRouteNetwork.HH_1, TestRouteNetwork.HH_2 })
            {
                RelatedInterestFilter = RelatedInterestFilterOptions.ReferencesFromRouteElementAndInterestObjects
            };
            
            Result<GetRouteNetworkDetailsResult> queryResult = await testNetwork.QueryApi.HandleAsync(routeNetworkQuery);

            // Assert that we got information back on all 4 network elements queried
            Assert.True(queryResult.IsSuccess);
            Assert.Equal(4, queryResult.Value.RouteNetworkElements.Count);

            // Assert that we got back one interest related to the 3 network elements that the walk of interest covers
            Assert.NotNull(queryResult.Value.Interests);
            Assert.Contains(queryResult.Value.Interests, i => i.Id == interestId);
            Assert.Equal(RouteNetworkInterestKindEnum.WalkOfInterest, queryResult.Value.Interests[interestId].Kind);
            Assert.Equal(3, queryResult.Value.Interests[interestId].RouteNetworkElementRefs.Count);
            Assert.Contains(TestRouteNetwork.CO_1, queryResult.Value.Interests[interestId].RouteNetworkElementRefs);
            Assert.Contains(TestRouteNetwork.S1, queryResult.Value.Interests[interestId].RouteNetworkElementRefs);
            Assert.Contains(TestRouteNetwork.HH_1, queryResult.Value.Interests[interestId].RouteNetworkElementRefs);

            // Assert that route element 1 (CO_1) has interest information with correct relation type
            Assert.NotNull(queryResult.Value.RouteNetworkElements[TestRouteNetwork.CO_1].InterestRelations);
            Assert.Contains(queryResult.Value.RouteNetworkElements[TestRouteNetwork.CO_1].InterestRelations, r => r.RefId == interestId && r.RelationKind == RouteNetworkInterestRelationKindEnum.Start);

            // Assert that route element 2 (S1) has interest information with correct relation type
            Assert.NotNull(queryResult.Value.RouteNetworkElements[TestRouteNetwork.S1].InterestRelations);
            Assert.Contains(queryResult.Value.RouteNetworkElements[TestRouteNetwork.S1].InterestRelations, r => r.RefId == interestId && r.RelationKind == RouteNetworkInterestRelationKindEnum.PassThrough);

            // Assert that route element 3 (HH_1) has interest information with correct relation type
            Assert.NotNull(queryResult.Value.RouteNetworkElements[TestRouteNetwork.HH_1].InterestRelations);
            Assert.Contains(queryResult.Value.RouteNetworkElements[TestRouteNetwork.HH_1].InterestRelations, r => r.RefId == interestId && r.RelationKind == RouteNetworkInterestRelationKindEnum.End);

            // Assert that route element 4 (HH_2) has no references to interest information
            Assert.Empty(queryResult.Value.RouteNetworkElements[TestRouteNetwork.HH_2].InterestRelations);
        }

        [Fact]
        public async void QueryReferencesFromRouteElementOnly_ShouldReturnInterestReferencesOnly()
        {
            // Create interest (CO_1) <- (S1) -> (HH_1) that we can then try query
            var interestId = Guid.NewGuid();
            var walk = new RouteNetworkElementIdList() { TestRouteNetwork.S1 };
            var createInterestCommand = new RegisterWalkOfInterest(interestId, walk);

            await testNetwork.CommandApi.HandleAsync(createInterestCommand);

            // Act: Query CO_1, S1, HH_1 and HH_2
            var routeNetworkQuery = new GetRouteNetworkDetails(new RouteNetworkElementIdList() { TestRouteNetwork.CO_1, TestRouteNetwork.S1, TestRouteNetwork.HH_1, TestRouteNetwork.HH_2 })
            {
                RelatedInterestFilter = RelatedInterestFilterOptions.ReferencesFromRouteElementOnly
            };

            Result<GetRouteNetworkDetailsResult> queryResult = await testNetwork.QueryApi.HandleAsync(routeNetworkQuery);

            // Assert that we got information back on all 4 network elements queried
            Assert.True(queryResult.IsSuccess);
            Assert.Equal(4, queryResult.Value.RouteNetworkElements.Count);

            // Assert that we did'nt get any interest object back
            Assert.Null(queryResult.Value.Interests);

            // Assert that route element 1 (CO_1) has interest information with correct relation type
            Assert.NotNull(queryResult.Value.RouteNetworkElements[TestRouteNetwork.CO_1].InterestRelations);
            Assert.Contains(queryResult.Value.RouteNetworkElements[TestRouteNetwork.CO_1].InterestRelations, r => r.RefId == interestId && r.RelationKind == RouteNetworkInterestRelationKindEnum.Start);

            // Assert that route element 2 (S1) has interest information with correct relation type
            Assert.NotNull(queryResult.Value.RouteNetworkElements[TestRouteNetwork.S1].InterestRelations);
            Assert.Contains(queryResult.Value.RouteNetworkElements[TestRouteNetwork.S1].InterestRelations, r => r.RefId == interestId && r.RelationKind == RouteNetworkInterestRelationKindEnum.PassThrough);

            // Assert that route element 3 (HH_1) has interest information with correct relation type
            Assert.NotNull(queryResult.Value.RouteNetworkElements[TestRouteNetwork.HH_1].InterestRelations);
            Assert.Contains(queryResult.Value.RouteNetworkElements[TestRouteNetwork.HH_1].InterestRelations, r => r.RefId == interestId && r.RelationKind == RouteNetworkInterestRelationKindEnum.End);

            // Assert that route element 4 (HH_2) has no references to interest information
            Assert.Empty(queryResult.Value.RouteNetworkElements[TestRouteNetwork.HH_2].InterestRelations);
        }

        [Fact]
        public async void QueryExplicitlyRequestingNoInterestInformation_ShouldReturnNoInterestInformation()
        {
            // Create interest (CO_1) <- (S1) -> (HH_1) that we can then try query
            var interestId = Guid.NewGuid();
            var walk = new RouteNetworkElementIdList() { TestRouteNetwork.S1 };
            var createInterestCommand = new RegisterWalkOfInterest(interestId, walk);

            await testNetwork.CommandApi.HandleAsync(createInterestCommand);

            // Act: Query CO_1, S1, HH_1 and HH_2
            var routeNetworkQuery = new GetRouteNetworkDetails(new RouteNetworkElementIdList() { TestRouteNetwork.CO_1, TestRouteNetwork.S1, TestRouteNetwork.HH_1, TestRouteNetwork.HH_2 })
            {
                RelatedInterestFilter = RelatedInterestFilterOptions.None
            };

            Result<GetRouteNetworkDetailsResult> queryResult = await testNetwork.QueryApi.HandleAsync(routeNetworkQuery);

            // Assert that we got information back on all 4 network elements queried
            Assert.True(queryResult.IsSuccess);
            Assert.Equal(4, queryResult.Value.RouteNetworkElements.Count);

            // Assert that we did'nt get any interest object back
            Assert.Null(queryResult.Value.Interests);

            // Assert that no route elements got interest relations
            Assert.Null(queryResult.Value.RouteNetworkElements[TestRouteNetwork.CO_1].InterestRelations);
            Assert.Null(queryResult.Value.RouteNetworkElements[TestRouteNetwork.S1].InterestRelations);
            Assert.Null(queryResult.Value.RouteNetworkElements[TestRouteNetwork.HH_1].InterestRelations);
            Assert.Null(queryResult.Value.RouteNetworkElements[TestRouteNetwork.HH_2].InterestRelations);
        }
    }
}
