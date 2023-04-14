using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.Collections.Generic;
using System.Net;
using CosmosIntegratedCacheTest.Models;
using System.Linq;
using CosmosIntegratedCacheTest.Services;

namespace CosmosIntegratedCacheTest.Controllers
{
    public class CosmosQueryWithDirectConnectionController
    {
        private const string CONTROLLER_TAG = "Cosmos Query With Direct Connection";
        private const string LIST_FUNCTION_REFIDS_NAME = "ListRefidsDirectConnection";
        private const string CREATE_FUNCTION_REFIDS_NAME = "CreateTestItemsByRefids";
        private readonly ITestItemService _testItemService;

        public CosmosQueryWithDirectConnectionController(ITestItemService testItemService)
        {
            _testItemService = testItemService ?? throw new ArgumentNullException(nameof(testItemService));
        }

        [OpenApiOperation(operationId: LIST_FUNCTION_REFIDS_NAME, tags: new[] { CONTROLLER_TAG },
            Summary = "List TestItems by refIds")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(List<string>))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TestItemQueryResult),
            Description = "A TestItemQueryResult object.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(TestItemQueryResult),
            Description = "RefIds in request not found.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(TestItemQueryResult),
            Description = "Unexpected error.")]
        [FunctionName(LIST_FUNCTION_REFIDS_NAME)]
        public async Task<IActionResult> ListRefidsDirectConnection(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "direct/test-items")]
            List<string> refIds,
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed {LIST_FUNCTION_REFIDS_NAME}.");

            TestItemQueryResult response = new TestItemQueryResult();
            try
            {
                response = await _testItemService.GetTestItemsByRefIdsDirectConnection(refIds);
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

        [OpenApiOperation(operationId: CREATE_FUNCTION_REFIDS_NAME, tags: new[] { CONTROLLER_TAG },
            Summary = "Create TestItems by refIds")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(List<string>))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(List<TestItem>),
            Description = "A list of TestItem objects created.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(TestItemQueryResult),
            Description = "Unexpected error.")]
        [FunctionName(CREATE_FUNCTION_REFIDS_NAME)]
        public async Task<IActionResult> CreateTestItemsByRefids(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "direct/test-items/new")]
            List<string> refIds,
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed {CREATE_FUNCTION_REFIDS_NAME}.");

            var testItems = new List<TestItem>();

            try
            {
                testItems = await _testItemService.CreateTestItemsByRefIdsDirectConnection(refIds);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }

            return new ObjectResult(testItems)
            {
                StatusCode = StatusCodes.Status201Created
            };
        }
    }
}
