using System.Collections.Generic;
using System.Threading.Tasks;
using CosmosIntegratedCacheTest.Models;

namespace CosmosIntegratedCacheTest.Services
{
    public interface ITestItemService
    {
        public Task<TestItemQueryResult> GetTestItemsByRefIdsDedicatedGateway(IEnumerable<string> refIds);
        public Task<TestItemQueryResult> GetTestItemsByRefIdsDirectConnection(IEnumerable<string> refIds);
        public Task<List<TestItem>> CreateTestItemsByRefIdsDirectConnection(IEnumerable<string> refIds);
        public Task<TestItemQueryResult> GetTestItemByIdAndPartitionKeyDedicatedGateway(string id, string partitionKey);
    }
}