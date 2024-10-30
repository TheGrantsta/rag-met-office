using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using rag_met_office.MetOffice;
using rag_met_office.TomorrowIo;

public class Utils
{
    public static string GetConfigurationValues(string keyName)
    {
        var builder = new ConfigurationBuilder().AddUserSecrets<Program>();

        IConfiguration configuration = builder.Build();

        return configuration[keyName] ?? "";
    }

    public static List<string> ExtractTextFromJson(string jsonData, bool isMetOffice)
    {
        return isMetOffice ? GetFromMetOffice(jsonData) : GetFromTomorrowIo(jsonData);
    }

    private static List<string> GetFromMetOffice(string jsonData)
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

                _ = stringBuilder.Append($"At {localDateTime.ToLocalTime()}, it will be ${GetTextFor(timeSeries.WeatherCode)} ");
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

    private static List<string> GetFromTomorrowIo(string jsonData)
    {
        var extractedTexts = new List<string>();

        try
        {
            var timelines = JsonConvert.DeserializeObject<Root>(jsonData);

            foreach (var minute in timelines.Timelines.Minutelys)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append($"At {minute.Time.ToLocalTime()}, it will be ${GetTextFor(minute.Values.WeatherCode)} ");
                stringBuilder.Append($"with max temperature of ${minute.Values.Temperature} ");
                stringBuilder.Append($"that will feel like ${minute.Values.FeelsLikeTemperature} ");
                stringBuilder.Append($"with {minute.Values.PrecipitationProbability}% chance of rain and total rainfall of {minute.Values.TotalPrecipitationAmount}mm");

                extractedTexts.Add(stringBuilder.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting text from JSON: {ex.Message}");
        }

        return extractedTexts;
    }

    private static string GetTextFor(int weatherCode)
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
}
