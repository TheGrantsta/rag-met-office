using Newtonsoft.Json;

namespace rag_met_office.MetOffice
{
    public class FeatureCollection
    {
        [JsonProperty("features")]
        public List<Feature> Features { get; set; } = [];
    }
}