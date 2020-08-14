using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using OpenFTTH.RouteNetworkService.Controllers;
using OpenFTTH.RouteNetworkService.Queries;
using OpenFTTH.RouteNetworkService.Repositories;
using System;
using System.IO;
using System.Threading;
using Xunit;

namespace OpenFTTH.RouteNetworkService.Tests
{
    public class QueryControlerTests
    {
        [Fact]
        public void ValidQueryRequestObject_MustReturnValidResultObject()
        {
            // Arrange
            var routeNodeQueryRequest = new RouteNodeQuery(Guid.NewGuid());

            var routeNodeQueryResult = new RouteNodeQueryResult()
            {
                RouteNodeId = routeNodeQueryRequest.RouteNodeId
            };

            var mockLog = new Mock<ILogger<QueryController>>();
            
            var mockRepo = new Mock<IRouteNodeRepository>();
            
            mockRepo.Setup(repo => repo.Query(routeNodeQueryRequest)).Returns(routeNodeQueryResult);

            var mockMediator = new Mock<IMediator>();

            mockMediator
                .Setup(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(routeNodeQueryResult)
                .Verifiable("Notification was not sent.");
            

            var mockRequest = HttpMockHelper.CreateMockRequest(routeNodeQueryRequest);
            var mockHttpContext = Mock.Of<HttpContext>(context => context.Request == mockRequest.Object);

            var controller = new QueryController(mockLog.Object, mockMediator.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockHttpContext
                }
            };


            // Act
            var result = controller.Get();


            // Assert
            Assert.True(result is ContentResult);

            var contentResult = result as ContentResult;

            Assert.Contains("json", contentResult.ContentType);
            Assert.Contains(routeNodeQueryRequest.RouteNodeId.ToString(), contentResult.Content);
        }

        [Fact]
        public void ProcessEmptyBodyRequest_ShouldReturnBadRequest()
        {
            // Arrange
            var routeNodeQueryRequest = new RouteNodeQuery(Guid.NewGuid());

            var routeNodeQueryResult = new RouteNodeQueryResult()
            {
                RouteNodeId = routeNodeQueryRequest.RouteNodeId
            };

            var mockLog = new Mock<ILogger<QueryController>>();

            var mockRepo = new Mock<IRouteNodeRepository>();

            mockRepo.Setup(repo => repo.Query(routeNodeQueryRequest)).Returns(routeNodeQueryResult);

            var mockMediator = new Mock<IMediator>();

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(x => x.Body).Returns(new MemoryStream());

            var mockHttpContext = Mock.Of<HttpContext>(context => context.Request == mockRequest.Object);

            var controller = new QueryController(mockLog.Object, mockMediator.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockHttpContext
                }
            };


            // Act
            var result = controller.Get();


            // Assert
            Assert.True(result is BadRequestObjectResult);

            var contentResult = result as BadRequestObjectResult;

            Assert.Contains("body is empty", contentResult.Value.ToString());
        }

        [Fact]
        public void ProcessInvalidJsonInBody_ShouldReturnBadRequest()
        {
            // Arrange
            var routeNodeQueryRequest = new RouteNodeQuery(Guid.NewGuid());

            var routeNodeQueryResult = new RouteNodeQueryResult()
            {
                RouteNodeId = routeNodeQueryRequest.RouteNodeId
            };

            var mockLog = new Mock<ILogger<QueryController>>();

            var mockRepo = new Mock<IRouteNodeRepository>();

            mockRepo.Setup(repo => repo.Query(routeNodeQueryRequest)).Returns(routeNodeQueryResult);

            var mockMediator = new Mock<IMediator>();

            var mockRequest = HttpMockHelper.CreateMockRequest("asdasdadsdsa");

            var mockHttpContext = Mock.Of<HttpContext>(context => context.Request == mockRequest.Object);

            var controller = new QueryController(mockLog.Object, mockMediator.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockHttpContext
                }
            };


            // Act
            var result = controller.Get();


            // Assert
            Assert.True(result is BadRequestObjectResult);
        }


      
    }
}
