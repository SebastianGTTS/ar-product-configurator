using Newtonsoft.Json;

namespace ARConfigurator
{
    public class Feature
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("isMandatory")]
        public bool IsMandatory { get; set; }

        [JsonProperty("hasOrSubfeatures")]
        public bool HasOrSubfeatures { get; set; }

        [JsonProperty("hasXorSubfeatures")]
        public bool HasXorSubfeatures { get; set; }

        [JsonProperty("isMaterial")]
        public bool IsMaterial { get; set; }

        [JsonProperty("isPhysical")]
        public bool IsPhysical { get; set; }

        [JsonProperty("requiringDependencyFrom")]
        public long[] RequiringDependencyFrom { get; set; }

        [JsonProperty("requiringDependencyTo")]
        public long[] RequiringDependencyTo { get; set; }

        [JsonProperty("excludingDependency")]
        public long[] ExcludingDependency { get; set; }

        [JsonProperty("features")]
        public Feature[] Features { get; set; }

        [JsonProperty("parentId", NullValueHandling = NullValueHandling.Ignore)]
        public long? ParentId { get; set; }

        [JsonProperty("material", NullValueHandling = NullValueHandling.Ignore)]
        public MaterialData Material { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public Metadata Metadata { get; set; }
    }
}
