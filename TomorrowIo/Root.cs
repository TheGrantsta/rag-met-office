using Newtonsoft.Json;

namespace rag_met_office.TomorrowIo
{
    public class Root
    {
        [JsonProperty("timelines")]
        public required Timelines Timelines { get; set; }
    }
}