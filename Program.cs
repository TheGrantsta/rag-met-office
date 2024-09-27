using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

class Program()
{
    static async Task Main(string[] args)
    {
        var retrievedData = await FetchDataFromApi("https://data.hub.api.metoffice.gov.uk/sitespecific/v0/point/hourly");

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

            Console.WriteLine("Retrieved Data:");
            Console.WriteLine(jsonResponse);
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

}
