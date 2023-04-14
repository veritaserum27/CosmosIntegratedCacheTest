using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CosmosIntegratedCacheTest.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;

namespace CosmosIntegratedCacheTest.Repositories
{
    public class CosmosDbRepository : ICosmosDbRepository
    {
        private readonly CosmosClient dedicatedGatewayCosmosClient;
        private readonly CosmosClient directCosmosClient;
        private readonly string databaseName;
        private readonly string containerName;
        private Container containerWithDedicatedConnection => dedicatedGatewayCosmosClient.GetDatabase(databaseName).GetContainer(containerName);
        private Container containerWithDirectConnection => directCosmosClient.GetDatabase(databaseName).GetContainer(containerName);
        
        public CosmosDbRepository(IConfiguration configuration)
        {
            var dedicatedConnectionString = configuration.GetConnectionString("COSMOS_GATEWAY_CONNECTION_STRING");

            if (String.IsNullOrEmpty(dedicatedConnectionString))
            {
                throw new ArgumentNullException(nameof(dedicatedConnectionString), "Missing COSMOS_GATEWAY_CONNECTION_STRING");
            }

            dedicatedGatewayCosmosClient = new CosmosClient(dedicatedConnectionString,
            new CosmosClientOptions()
            {
                ConnectionMode = ConnectionMode.Gateway,
                ConsistencyLevel = ConsistencyLevel.Eventual
            });

            var directConnectionString = configuration.GetConnectionString("COSMOS_CONNECTION_STRING");

            if (String.IsNullOrEmpty(directConnectionString))
            {
                throw new ArgumentNullException(nameof(directConnectionString), "Missing COSMOS_CONNECTION_STRING");
            }

            directCosmosClient = new CosmosClient(directConnectionString);

            databaseName = configuration.GetValue<string>("COSMOS_DBNAME");

            if (String.IsNullOrEmpty(databaseName))
            {
                throw new ArgumentNullException(nameof(databaseName), "Missing COSMOS_DBNAME");
            }

            containerName = configuration.GetValue<string>("COSMOS_CONTAINER_NAME");

            if (String.IsNullOrEmpty(containerName))
            {
                throw new ArgumentNullException(nameof(containerName), "Missing COSMOS_CONTAINER_NAME");
            }
        }

        public async Task<TestItemQueryResult> GetTestItemsByRefIdsDedicatedGateway(IEnumerable<string> refIds, int cacheStaleness)
        {
            if (refIds == null) throw new ArgumentNullException(nameof(refIds));
            if (!refIds.Any()) throw new ArgumentException(nameof(refIds));

            List<TestItem> testItems = new List<TestItem>();
            TestItemQueryResult result = new TestItemQueryResult();

            var sortedRefIds = refIds.OrderBy(id => id).ToList();

            IQueryable<TestItem> queryable = containerWithDedicatedConnection.GetItemLinqQueryable<TestItem>(
                 requestOptions: new QueryRequestOptions
                 {
                     DedicatedGatewayRequestOptions = new DedicatedGatewayRequestOptions()
                     {
                         MaxIntegratedCacheStaleness = TimeSpan.FromHours(cacheStaleness)
                     },
                     ConsistencyLevel = ConsistencyLevel.Eventual
                 }
             );

            queryable = queryable.Where(e => sortedRefIds.Contains(e.RefId));

            using (var linqFeed = queryable.ToFeedIterator())
            {
                while (linqFeed.HasMoreResults)
                {
                    var feedResponse = await linqFeed.ReadNextAsync();

                    result.RequestUnits = feedResponse.RequestCharge;

                    foreach (var item in feedResponse)
                    {
                        testItems.Add(item);
                    }
                }
            }

            result.TestItems = testItems;

            return result;
        }

        public async Task<TestItemQueryResult> GetTestItemsByRefIdsDirectConnection(IEnumerable<string> refIds)
        {
            if (refIds == null) throw new ArgumentNullException(nameof(refIds));
            if (!refIds.Any()) throw new ArgumentException(nameof(refIds));

            List<TestItem> testItems = new List<TestItem>();
            TestItemQueryResult result = new TestItemQueryResult();

            var sortedRefIds = refIds.OrderBy(id => id).ToList();

            IQueryable<TestItem> queryable = containerWithDirectConnection.GetItemLinqQueryable<TestItem>();

            queryable = queryable.Where(e => sortedRefIds.Contains(e.RefId));

            using (var linqFeed = queryable.ToFeedIterator())
            {
                while (linqFeed.HasMoreResults)
                {
                    var feedResponse = await linqFeed.ReadNextAsync();

                    result.RequestUnits = feedResponse.RequestCharge;

                    foreach (var item in feedResponse)
                    {
                        testItems.Add(item);
                    }
                }
            }

            result.TestItems = testItems;

            return result;
        }

        public async Task<List<TestItem>> CreateTestItemsByRefIdsDirectConnection(IEnumerable<string> refIds)
        {
            if (refIds == null) throw new ArgumentNullException(nameof(refIds));
            if (!refIds.Any()) throw new ArgumentException(nameof(refIds));
            
            List<Task> tasks = new List<Task>();
            List<TestItem> items = new List<TestItem>();

            foreach (var refId in refIds)
            {
                var item = new TestItem()
                {
                    Id = Guid.NewGuid().ToString(),
                    RefId = refId,
                    CustomKey = Guid.NewGuid().ToString()
                };

                items.Add(item);

                tasks.Add(containerWithDirectConnection.CreateItemAsync(
                    item,
                    new PartitionKey(item.CustomKey))
                    .ContinueWith(itemResponse =>
                    {
                        if (!itemResponse.IsCompletedSuccessfully)
                        {
                            AggregateException innerExceptions = itemResponse.Exception.Flatten();
                            if (innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
                            {
                                throw new CosmosException($"Received {cosmosException.StatusCode} ({cosmosException.Message}).",
                                                            cosmosException.StatusCode,
                                                            cosmosException.SubStatusCode,
                                                            cosmosException.ActivityId,
                                                            cosmosException.RequestCharge);
                            }
                            else
                            {
                                throw new Exception($"Exception {innerExceptions.InnerExceptions.FirstOrDefault()}.");
                            }
                        }
                    }));
            }

            // Wait until all are done
            await Task.WhenAll(tasks);

            return items;
        }

        public async Task<TestItemQueryResult> GetTestItemDedicatedGateway(string id, string partitionKey, int cacheStaleness)
        {
            if (String.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
            if (String.IsNullOrEmpty(partitionKey)) throw new ArgumentNullException(nameof(partitionKey));

            ItemResponse<TestItem> response = await containerWithDedicatedConnection.ReadItemAsync<TestItem>(id, new PartitionKey(partitionKey), 
            new ItemRequestOptions() 
            {
                DedicatedGatewayRequestOptions = new DedicatedGatewayRequestOptions()
                {
                    MaxIntegratedCacheStaleness = TimeSpan.FromHours(cacheStaleness)
                },
                ConsistencyLevel = ConsistencyLevel.Eventual
            });

            var testItemQueryResult = new TestItemQueryResult();

            testItemQueryResult.TestItems.Add(response.Resource);
            testItemQueryResult.RequestUnits = response.RequestCharge;

            return testItemQueryResult;
        }
    }
}