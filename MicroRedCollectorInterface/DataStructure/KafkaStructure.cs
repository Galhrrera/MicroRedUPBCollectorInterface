using Newtonsoft.Json;
using System.Collections.Generic;

namespace DataStructure
{
    public class KafkaStructure
    {
        [JsonProperty(propertyName: "schema")]
        public Schema Schema { get; set; }

        [JsonProperty(propertyName: "payload")]
        public Dictionary<string, double> Payload { get; set; }
    }

    public class Schema
    {
        [JsonProperty(propertyName: "type")]
        public string Type { get; set; }

        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

        [JsonProperty(propertyName: "fields")]
        public List<string> Fields { get; set; }
    }

}
