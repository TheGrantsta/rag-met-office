using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Newtonsoft.Json;

public class OpenAiApi
{
    public static async Task<string> GenerateResponseBasedOnContext(string context, string latitude, string longitude, bool isMetOffice)
    {
        var generatedText = string.Empty;

        string contextInfo = isMetOffice ?
        $"Here is the weather data for this location (latitude {latitude} and longitude {longitude}) for the next 24 hours: {context}." :
        $"Here is the weather data for this location (latitude {latitude} and longitude {longitude}) for the next hour: {context}.";

        string prompt = isMetOffice ?
        "What will the weather be like for the next 4 hours? Summarise the response to two lines, to a maximum of 30 words, and round temperatures to zero decimal places." :
        "Summarise and identify the weather for the next hour focusing on any changes. Limit response to a maximum of 30 words and round temperatures to zero decimal places.";

        var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = contextInfo },
                    new { role = "user", content = prompt }
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