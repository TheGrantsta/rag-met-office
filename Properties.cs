using Newtonsoft.Json;
public class Properties
{
    [JsonProperty("timeSeries")]
    public List<TimeSeries> TimeSeries { get; set; } = [];
}