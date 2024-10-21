using Newtonsoft.Json;

public class TimeSeries
{
    [JsonProperty("time")]
    public string? Time { get; set; }

    [JsonProperty("maxScreenAirTemp")]
    public double MaxScreenAirTemp { get; set; }

    [JsonProperty("minScreenAirTemp")]
    public double MinScreenAirTemp { get; set; }

    [JsonProperty("feelsLikeTemperature")]
    public double FeelsLikeTemperature { get; set; }

}