using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Newtonsoft.Json;

class Program()
{
    static async Task Main(string[] args)
    {
        var hourlySpotData = await FetchDataFromApi("https://data.hub.api.metoffice.gov.uk/sitespecific/v0/point/hourly");

        var hourlySpotDataAsText = ExtractTextFromJson(hourlySpotData);

        if(hourlySpotDataAsText.Count > 0)
        {
            var generatedResponse = await GenerateResponseBasedOnContext(hourlySpotDataAsText);

            Console.WriteLine($"\n{generatedResponse}\n");
        }
    }

     static async Task<string> FetchDataFromApi(string apiUrl)
    {
        var jsonResponse = string.Empty;

        try
        {
            var requestUrl = $"{apiUrl}?latitude=51.652931&longitude=-0.199610";

            using (var client = new HttpClient()) {
                client.DefaultRequestHeaders.Add("ApiKey", Utils.GetConfigurationValues("MetOfficeApiKey"));

                var response = await client.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                jsonResponse = await response.Content.ReadAsStringAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving weather data from API: {ex.Message}");
        }

        return jsonResponse;
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
                extractedTexts.Add($"At {localDateTime.ToLocalTime()}, it will be cloudy with max temperature of ${timeSeries.MaxScreenAirTemp}, a min of ${timeSeries.MinScreenAirTemp} and will feel like ${timeSeries.FeelsLikeTemperature}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting text from JSON: {ex.Message}");
        }            
        
        return extractedTexts;
    }

       static async Task<string> GenerateResponseBasedOnContext(List<string> strings)
    {
        // For simplicity, we are just going to concatenate the embeddings into a simple context for now.
        // In a real system, you would perform a similarity search to find the most relevant embeddings.
        var context = string.Join("\n", strings.Select(e => string.Join(", ", e)));

        return await GenerateResponseBasedOnContext(context);
    }

    static async Task<string> GenerateResponseBasedOnContext(string context)
    {
        var generatedText = string.Empty;

        string contextInfo = $"Here is the weather data for Barnet for the next 24 hours: {context}";

        var requestBody = new
            {
                model = "gpt-3.5-turbo", // Or another model like "gpt-3.5-turbo"
                messages = new[]
                {
                    new { role = "system", content = contextInfo },
                    new { role = "user", content = "What will the weather be like in Barnet for the next 4 hours? Summarise the response to two lines and round temperatures to zero decimal places" }
                },
                max_tokens = 100,
                temperature = 0.7
            };

        try
        {
             using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Utils.GetConfigurationValues("OpenAiApiKey"));
                client.DefaultRequestHeaders.Add("OpenAI-Organization", Utils.GetConfigurationValues("OpenAiOrganisationId"));
                client.DefaultRequestHeaders.Add("OpenAI-Project", Utils.GetConfigurationValues("OpenAiProjectId"));

                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                // Send the POST request to OpenAI's API
                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);

                response.EnsureSuccessStatusCode();

                // Read and process the response
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var parsedResponse = JsonDocument.Parse(jsonResponse);

                // Extract the generated text from the response
                generatedText = parsedResponse.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").ToString().Trim();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating response: {ex.Message}");
        }

        return generatedText;
    }
}
