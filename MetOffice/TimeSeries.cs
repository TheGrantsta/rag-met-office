using Newtonsoft.Json;

namespace rag_met_office.MetOffice
{
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

        [JsonProperty("significantWeatherCode")]
        public int WeatherCode { get; set; }

        [JsonProperty("totalPrecipAmount")]
        public double TotalPrecipitationAmount { get; set; }

        [JsonProperty("probOfPrecipitation")]
        public int ProbabilityOfPrecipitation { get; set; }
    }
}