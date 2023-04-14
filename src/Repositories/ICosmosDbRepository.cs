using System.Collections.Generic;
using System.Threading.Tasks;
using CosmosIntegratedCacheTest.Models;

namespace CosmosIntegratedCacheTest.Repositories
{
    public interface ICosmosDbRepository
    {
        public Task<TestItemQueryResult> GetTestItemsByRefIdsDedicatedGateway(IEnumerable<string> refIds, int cacheStaleness);
        public Task<TestItemQueryResult> GetTestItemsByRefIdsDirectConnection(IEnumerable<string> refIds);
        public Task<List<TestItem>> CreateTestItemsByRefIdsDirectConnection(IEnumerable<string> refIds);
        public Task<TestItemQueryResult> GetTestItemDedicatedGateway(string id, string partitionKey, int cacheStaleness);
    }
}