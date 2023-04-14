using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.Collections.Generic;
using System.Net;
using CosmosIntegratedCacheTest.Models;
using System.Linq;
using CosmosIntegratedCacheTest.Services;

namespace CosmosIntegratedCacheTest.Controllers
{
    public class CosmosQueryWithDedicatedGatewayController
    {
        private const string CONTROLLER_TAG = "Cosmos Query With Dedicated Gateway";
        private const string LIST_FUNCTION_REFIDS_NAME = "ListRefidsDedicatedGateway";
        private const string LIST_FUNCTION_REFIDS_RETRY_NAME = "ListRefidsDedicatedGatewayWithRetry";
        private const string GET_REFID_FUNCTION = "GetSingleRefIdDedicatedGateway";
        private readonly ITestItemService _testItemService;

        public CosmosQueryWithDedicatedGatewayController(IConfiguration configuration, ITestItemService testItemService)
        {
            _testItemService = testItemService ?? throw new ArgumentNullException(nameof(testItemService));
        }

        [OpenApiOperation(operationId: LIST_FUNCTION_REFIDS_NAME, tags: new[] { CONTROLLER_TAG },
            Summary = "List TestItems by refIds.")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(List<string>))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TestItemQueryResult),
            Description = "A TestItemQueryResult object.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(TestItemQueryResult),
            Description = "RefIds in request not found.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(TestItemQueryResult),
            Description = "Unexpected error.")]
        [FunctionName(LIST_FUNCTION_REFIDS_NAME)]
        public async Task<IActionResult> ListRefidsDedicatedGateway(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "gateway/test-items")]
            List<string> refIds,
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed {LIST_FUNCTION_REFIDS_NAME}.");

            var response = new TestItemQueryResult();
            try
            {
                response = await _testItemService.GetTestItemsByRefIdsDedicatedGateway(refIds);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }

            if (!response.TestItems.Any())
            {  
                return new NotFoundObjectResult(response);
            }

            return new OkObjectResult(response);
        }

        [OpenApiOperation(operationId: LIST_FUNCTION_REFIDS_RETRY_NAME, tags: new[] { CONTROLLER_TAG },
            Summary = "List TestItems by refIds with retry if no items returned from cache.")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(List<string>))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TestItemQueryResult),
            Description = "A TestItemQueryResult object.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(TestItemQueryResult),
            Description = "RefIds in request not found.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(TestItemQueryResult),
            Description = "Unexpected error.")]
        [FunctionName(LIST_FUNCTION_REFIDS_RETRY_NAME)]
        public async Task<IActionResult> ListRefidsDedicatedGatewayWithRetry(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "gateway/test-items-retry")]
            List<string> refIds,
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed {LIST_FUNCTION_REFIDS_RETRY_NAME}.");

            List<TestItemQueryResult> response = new List<TestItemQueryResult>();
            
            try
            {
                response = await _testItemService.GetTestItemsByRefIdsDedicatedGatewayWithRetry(refIds);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }

            if (!response.Any(x => x.TestItems.Any()))
            {  
                return new NotFoundObjectResult(response);
            }

            return new OkObjectResult(response);
        }


        [OpenApiOperation(operationId: GET_REFID_FUNCTION, tags: new[] { CONTROLLER_TAG },
            Summary = "Get single TestItem using a point read.")]
        [OpenApiParameter(name: "id")]
        [OpenApiParameter(name: "partitionKey")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TestItemQueryResult),
            Description = "A TestItemQueryResult object.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(TestItemQueryResult),
            Description = "RefId in request not found.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(TestItemQueryResult),
            Description = "Unexpected error.")]
        [FunctionName(GET_REFID_FUNCTION)]
        public async Task<IActionResult> GetSingleRefIdDedicatedGateway(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gateway/test-item/{id}/{partitionKey}")]
            HttpRequest req,
            string id,
            string partitionKey,
            ILogger log
        )
        {
            log.LogInformation($"C# HTTP trigger function processed {GET_REFID_FUNCTION}.");

            var response = new TestItemQueryResult();

            try
            {
                response = await _testItemService.GetTestItemByIdAndPartitionKeyDedicatedGateway(id, partitionKey);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }

            if (!response.TestItems.Any())
            {  
                return new NotFoundObjectResult(response);
            }

            return new OkObjectResult(response);
        }
    }
}
