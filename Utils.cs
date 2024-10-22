using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

public class Utils
{
    public static string GetConfigurationValues(string keyName)
    {
        var builder = new ConfigurationBuilder().AddUserSecrets<Program>();

        IConfiguration configuration = builder.Build();

        return configuration[keyName] ?? "";
    }

    public static string Boo(int weatherCode)
    {
        Dictionary<int, string> weathers = new Dictionary<int, string>
        {
            { 0, "clear night" },
            { 1, "sunny day" },
            { 2, "partly cloudy night" },
            { 3, "sunny intervals" },
            { 4, "" }, //?
            { 5, "mist" },
            { 6, "fog" },
            { 7, "cloudy" },
            { 8, "overcast" },
            { 9, "light shower night" },
            { 10, "" }, //?
            { 11, "" }, //?
            { 12, "" }, //?
            { 13, "" }, //?
            { 14, "" }, //?
            { 15, "" }, //?
            { 16, "" }, //?
            { 17, "" }, //?
            { 18, "" }, //?
            { 19, "" }, //?
            { 20, "" }, //?
            { 21, "" }, //?
            { 22, "" }, //?
            { 23, "" }, //?
            { 24, "" }, //?
            { 25, "" }, //?
            { 26, "" }, //?
            { 27, "" }, //?
            { 28, "" }, //?
            { 29, "" }, //?
            { 30, "" }, //?
            { 31, "" } //?
        };

        if (!weathers.TryGetValue(weatherCode, out string? weather))
        {
            weather = "unknown";
        };

        return weather;
    }

    public static List<string> ExtractTextFromJson(string jsonData)
    {
        var extractedTexts = new List<string>();

        try
        {
            var featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(jsonData);

            var properties = featureCollection.Features.First().Properties;

            foreach (var timeSeries in properties.TimeSeries)
            {
                var stringBuilder = new StringBuilder();
                var localDateTime = DateTime.Parse(timeSeries.Time, null, System.Globalization.DateTimeStyles.RoundtripKind);

                stringBuilder.Append($"At {localDateTime.ToLocalTime()}, it will be ${Utils.Boo(timeSeries.WeatherCode)} ");
                stringBuilder.Append($"with max temperature of ${timeSeries.MaxScreenAirTemp} and ");
                stringBuilder.Append($"a min of ${timeSeries.MinScreenAirTemp} ");
                stringBuilder.Append($"that will feel like ${timeSeries.FeelsLikeTemperature} ");
                stringBuilder.Append($"with {timeSeries.ProbabilityOfPrecipitation}% chance of rain and total rainfall of {timeSeries.TotalPrecipitationAmount}mm");

                extractedTexts.Add(stringBuilder.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting text from JSON: {ex.Message}");
        }            
        
        return extractedTexts;
    }
}
