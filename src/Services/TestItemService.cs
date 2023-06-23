using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CosmosIntegratedCacheTest.Models;
using CosmosIntegratedCacheTest.Repositories;
using Microsoft.Extensions.Configuration;

namespace CosmosIntegratedCacheTest.Services
{
    public class TestItemService : ITestItemService
    {
        private readonly int cacheDuration;
        private readonly ICosmosDbRepository _cosmosDbRepository;
        public TestItemService(IConfiguration configuration, ICosmosDbRepository cosmosDbRepository)
        {
            _cosmosDbRepository = cosmosDbRepository ?? throw new ArgumentNullException(nameof(cosmosDbRepository));

            var cacheDurationString = configuration.GetValue<string>("QUERY_CACHE_DURATION_HOURS");

            if (String.IsNullOrEmpty(cacheDurationString))
            {
                throw new ArgumentNullException(nameof(cacheDurationString), "Missing QUERY_CACHE_DURATION_HOURS");
            }

            if (!Int32.TryParse(cacheDurationString, out cacheDuration))
            {
                throw new ArgumentException("Invalid value for QUERY_CACHE_DURATION_DAYS. Must be parsable to int.");
            }
        }

        public async Task<List<TestItem>> CreateTestItemsByRefIdsDirectConnection(IEnumerable<string> refIds)
        {
            if (refIds == null) throw new ArgumentNullException(nameof(refIds));
            if (!refIds.Any()) throw new ArgumentException(nameof(refIds));

            return await _cosmosDbRepository.CreateTestItemsByRefIdsDirectConnection(refIds);
        }

        public async Task<TestItemQueryResult> GetTestItemByIdAndPartitionKeyDedicatedGateway(string id, string partitionKey)
        {
            if (String.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
            if (String.IsNullOrEmpty(partitionKey)) throw new ArgumentNullException(nameof(partitionKey));

            return await _cosmosDbRepository.GetTestItemDedicatedGateway(id, partitionKey, cacheDuration);
        }

        public async Task<TestItemQueryResult> GetTestItemsByRefIdsDedicatedGateway(IEnumerable<string> refIds)
        {
            if (refIds == null) throw new ArgumentNullException(nameof(refIds));
            if (!refIds.Any()) throw new ArgumentException(nameof(refIds));
            
            return await _cosmosDbRepository.GetTestItemsByRefIdsDedicatedGateway(refIds, cacheDuration);
        }

        public async Task<TestItemQueryResult> GetTestItemsByRefIdsDirectConnection(IEnumerable<string> refIds)
        {
            if (refIds == null) throw new ArgumentNullException(nameof(refIds));
            if (!refIds.Any()) throw new ArgumentException(nameof(refIds));
            
            return await _cosmosDbRepository.GetTestItemsByRefIdsDirectConnection(refIds);
        }
    }
}