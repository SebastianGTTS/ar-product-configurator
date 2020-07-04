using Newtonsoft.Json;

namespace ARConfigurator
{
    public class MaterialData
    {
        public static MaterialData FromJson(string json) => JsonConvert.DeserializeObject<MaterialData>(json, Converter.Settings);

        [JsonProperty("textureFilename")]
        public string TextureFilename { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }
    }
}
