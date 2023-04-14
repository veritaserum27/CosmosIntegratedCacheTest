using System.Collections.Generic;
using Newtonsoft.Json;

namespace CosmosIntegratedCacheTest.Models
{
    public class TestItemQueryResult
    {
        [JsonProperty("testItems")]
        public List<TestItem> TestItems { get; set; } = new List<TestItem>();

        [JsonProperty("requestUnits")]
        public double RequestUnits { get; set; }
    }
}