using Newtonsoft.Json;

namespace rag_met_office.TomorrowIo
{
    public class ForecastValues
    {
        [JsonProperty("precipitationProbability")]
        public int PrecipitationProbability { get; set; }

        [JsonProperty("weatherCode")]
        public int WeatherCode { get; set; }

        [JsonProperty("temperature")]
        public double Temperature { get; set; }

        [JsonProperty("temperatureApparent")]
        public double FeelsLikeTemperature { get; set; }

        [JsonProperty("rainIntensity")]
        public double TotalPrecipitationAmount { get; set; }
    }
}