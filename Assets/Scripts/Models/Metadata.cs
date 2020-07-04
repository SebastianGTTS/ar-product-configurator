using Newtonsoft.Json;
using System.Collections.Generic;

namespace ARConfigurator
{
    public class Metadata
    {
        [JsonProperty("brand")]
        public string Brand { get; set; }

        [JsonProperty("modelFilename")]
        public string ModelFilename { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("leftSlot")]
        public List<long> LeftSlot { get; set; }

        [JsonProperty("rightSlot")]
        public List<long> RightSlot { get; set; }

        [JsonProperty("upperSlot")]
        public List<long> UpperSlot { get; set; }
    }
}
