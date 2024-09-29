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

        if (hourlySpotDataAsText.Any())
        {
            var embeddings = await GenerateEmbeddings(hourlySpotDataAsText);

            if (embeddings.Any())
            {
                var generatedResponse = await GenerateResponseBasedOnContext(embeddings);

                Console.WriteLine($"Generated Response: {generatedResponse}");
            }
        }

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

    static async Task<List<float[]>> GenerateEmbeddings(List<string> texts)
    {
        var apiKey = GetConfigurationValues("OpenAiApiKey");
        var organisationId = GetConfigurationValues("OpenAiOrganisationId");
        var projectId = GetConfigurationValues("OpenAiProjectId");

        var embeddingsList = new List<float[]>();

        foreach (var text in texts)
        {
            var requestBody = new
            {
                input = text,
                model = "text-embedding-ada-002" // The OpenAI embeddings model
            };

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    client.DefaultRequestHeaders.Add("OpenAI-Organization", organisationId);
                    client.DefaultRequestHeaders.Add("OpenAI-Project", projectId);

                    var response = await client.PostAsync("https://api.openai.com/v1/embeddings", 
                        new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"));
                    
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var parsedResponse = JsonDocument.Parse(jsonResponse);

                    var embeddingData = parsedResponse.RootElement
                        .GetProperty("data")[0]
                        .GetProperty("embedding")
                        .EnumerateArray()
                        .Select(x => x.GetSingle())
                        .ToArray();

                    embeddingsList.Add(embeddingData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating embedding: {ex.Message}");
            }
        }

        return embeddingsList;
    }

    static async Task<string> GenerateResponseBasedOnContext(List<float[]> embeddings)
    {
        var generatedText = String.Empty;

        // For simplicity, we are just going to concatenate the embeddings into a simple context for now.
        // In a real system, you would perform a similarity search to find the most relevant embeddings.
        var context = string.Join("\n", embeddings.Select(e => string.Join(", ", e)));

        var apiKey = GetConfigurationValues("OpenAiApiKey");
        var organisationId = GetConfigurationValues("OpenAiOrganisationId");
        var projectId = GetConfigurationValues("OpenAiProjectId");

        var requestBody = new
        {
            model = "gpt-4", // Use the GPT model
            prompt = $"Using the following embeddings as context:\n{context}\nGenerate a relevant response:",
            max_tokens = 150,
            temperature = 0.7
        };

        try
        {
             using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.DefaultRequestHeaders.Add("OpenAI-Organization", organisationId);
                client.DefaultRequestHeaders.Add("OpenAI-Project", projectId);

                // Send the POST request to OpenAI's API
                var response = await client.PostAsync("https://api.openai.com/v1/completions",
                    new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"));

                response.EnsureSuccessStatusCode();

                // Read and process the response
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var parsedResponse = JsonDocument.Parse(jsonResponse);

                // Extract the generated text from the response
                generatedText = parsedResponse.RootElement.GetProperty("choices")[0].GetProperty("text").ToString().Trim();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating response: {ex.Message}");
        }

        return generatedText;
    }
}
