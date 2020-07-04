using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ARConfigurator
{
    public class FeatureModel
    {
        public static FeatureModel FromJson(string json) => JsonConvert.DeserializeObject<FeatureModel>(json, Converter.Settings);

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("features")]
        public Feature[] Features { get; set; }

        [JsonIgnore]
        public Dictionary<long, Feature> FeatureMap { get; set; }

        [OnDeserialized]
        internal void PostProcess(StreamingContext context)
        {
            FeatureMap = new Dictionary<long, Feature>();

            var queue = new Queue<Feature>(Features);
            while (queue.Count > 0)
            {
                Feature currentFeature = queue.Dequeue();

                FeatureMap.Add(currentFeature.Id, currentFeature);
                foreach (Feature subfeature in currentFeature.Features)
                {
                    subfeature.ParentId = currentFeature.Id;
                    queue.Enqueue(subfeature);
                }
            }
        }
    }
}
