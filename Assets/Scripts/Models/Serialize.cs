using Newtonsoft.Json;

namespace ARConfigurator
{
    public static class Serialize
    {
        public static string MaterialData(MaterialData data) => JsonConvert.SerializeObject(data, Converter.Settings);
    }
}
