using System;
using Newtonsoft.Json;

namespace CosmosIntegratedCacheTest.Models
{
    public class TestItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("customKey")]
        public string CustomKey { get; set; }

        [JsonProperty("refId")]
        public string RefId { get; set; }
    }
}