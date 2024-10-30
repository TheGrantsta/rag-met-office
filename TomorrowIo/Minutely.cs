using Newtonsoft.Json;

namespace rag_met_office.TomorrowIo
{
    public class Minutely
    {
        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("values")]
        public required ForecastValues Values { get; set; }
    }
}