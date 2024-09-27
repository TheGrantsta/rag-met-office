using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

class Program()
{
    static async Task Main(string[] args)
    {
        var hourlySpotData = await FetchDataFromApi("https://data.hub.api.metoffice.gov.uk/sitespecific/v0/point/hourly");

        var hourlySpotDataAsText = ExtractTextFromJson(hourlySpotData);

        //Debugging purposes only!
        // foreach(var text in hourlySpotDataAsText)
        // {
        //     Console.WriteLine($"Text: {text}");
        // }

        Console.WriteLine("Finished!");
    }

     static async Task<string> FetchDataFromApi(string apiUrl)
    {
        var jsonResponse = String.Empty;

        try
        {
            var requestUrl = $"{apiUrl}?latitude=51.652931&longitude=-0.199610";
            var metOfficeApiKey = GetConfigurationValues("MetOfficeApiKey");

            using (var client = new HttpClient()) {
                client.DefaultRequestHeaders.Add("ApiKey", metOfficeApiKey);

                var response = await client.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                jsonResponse = await response.Content.ReadAsStringAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving data from API: {ex.Message}");
        }

        return jsonResponse;
    }

    
    private static string GetConfigurationValues(string keyName)
    {
        var builder = new ConfigurationBuilder().AddUserSecrets<Program>();

        IConfiguration configuration = builder.Build();

        return configuration[keyName]??"";
    }

    private static List<string> ExtractTextFromJson(string jsonData)
    {
        var extractedTexts = new List<string>();

        try
        {
            var featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(jsonData);

            var properties = featureCollection.Features.First().Properties;

            foreach (var timeSeries in properties.TimeSeries)
            {
                var localDateTime = DateTime.Parse(timeSeries.Time, null, System.Globalization.DateTimeStyles.RoundtripKind);
                extractedTexts.Add($"Maximum temperature {timeSeries.MaxScreenAirTemp} and minimum temperature {timeSeries.MinScreenAirTemp} at {localDateTime.ToLocalTime()}" );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting text from JSON: {ex.Message}");
        }            
        
        return extractedTexts;
    }
}

public class FeatureCollection
{
    [JsonProperty("features")]
    public List<Feature> Features { get; set; } = new List<Feature>();
}

public class Feature
{
    [JsonProperty("properties")]
    public Properties? Properties {get; set;}
}

public class Properties
{
    [JsonProperty("timeSeries")]
    public List<TimeSeries> TimeSeries { get; set; } = new List<TimeSeries>();
}

public class TimeSeries
{
    [JsonProperty("time")]
    public string? Time { get; set; }

    [JsonProperty("maxScreenAirTemp")]
    public double MaxScreenAirTemp { get; set; }

    [JsonProperty("minScreenAirTemp")]
    public double MinScreenAirTemp { get; set; }

}
