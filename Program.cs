using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Newtonsoft.Json;

class Program()
{
    public struct Coordinates(string latitude, string longitude)
    {
        public string Latitude { get; set; } = latitude;
        public string Longitude { get; set; } = longitude;
    }

    private static Coordinates _coordinates = new("51.652931", "-0.199610");

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
            var requestUrl = $"{apiUrl}?latitude={_coordinates.Latitude}&longitude={_coordinates.Longitude}";

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
                extractedTexts.Add($"At {localDateTime.ToLocalTime()}, it will be ${Utils.Boo(timeSeries.WeatherCode)} with max temperature of ${timeSeries.MaxScreenAirTemp}, a min of ${timeSeries.MinScreenAirTemp} and will feel like ${timeSeries.FeelsLikeTemperature}");
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
        // For simplicity, we are just going to concatenate the weather statements into a simple context for now.
        var context = string.Join("\n", strings.Select(e => string.Join(". ", e)));

        return await GenerateResponseBasedOnContext(context);
    }

    static async Task<string> GenerateResponseBasedOnContext(string context)
    {
        var generatedText = string.Empty;

        string contextInfo = $"Here is the weather data for this location (latitude {_coordinates.Latitude} and longitude {_coordinates.Longitude}) for the next 24 hours: {context}.";

        var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = contextInfo },
                    new { role = "user", content = $"What will the weather be like for the next 4 hours? Summarise the response to two lines, to a maximum of 30 words, and round temperatures to zero decimal places." }
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

                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var parsedResponse = JsonDocument.Parse(jsonResponse);

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
