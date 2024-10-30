using Newtonsoft.Json;

namespace rag_met_office.MetOffice
{
    public class Properties
    {
        [JsonProperty("timeSeries")]
        public List<TimeSeries> TimeSeries { get; set; } = [];
    }
}