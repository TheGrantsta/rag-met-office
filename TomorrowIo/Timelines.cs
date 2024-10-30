using Newtonsoft.Json;

namespace rag_met_office.TomorrowIo
{
    public class Timelines
    {
        [JsonProperty("minutely")]
        public List<Minutely> Minutelys { get; set; } = [];
    }
}