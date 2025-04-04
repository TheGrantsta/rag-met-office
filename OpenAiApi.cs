using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Newtonsoft.Json;
using static ForecastApi;

public class OpenAiApi
{
    public static async Task<string> GenerateResponseBasedOnContext(ForecastApiParams forecastApiParams, string context)
    {
        var generatedText = string.Empty;

        string contextInfo = forecastApiParams.GetIsMetOffice() ?
            $"Here is the weather data for this location (latitude {forecastApiParams.Latitude} and longitude {forecastApiParams.Longitude}) for the next 24 hours: {context}." :
            $"Here is the weather data for this location (latitude {forecastApiParams.Latitude} and longitude {forecastApiParams.Longitude}) for the next hour: {context}.";

        string prompt = forecastApiParams.GetIsMetOffice() ?
            "You are an experienced meterologist at the Met Office and I would your professional opinion on what will the weather be like for the next 4 hours? Summarise the response to two lines, to a maximum of 30 words, and round temperatures to zero decimal places. Please don't make things up and check your response, but don't include your checks in the response." :
            "You are an experienced meterologist at the Met Office and I would your professional opinion to summarise and identify the weather for the next hour focusing on any changes like it will start or stop raining. If there is rain in the forecast, could you how many minutes before it starts or stops? Limit response to a maximum of 30 words and round temperatures to zero decimal places. Please don't make things up and check your response, but don't include your checks in the response.";

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

                var promptTokens = parsedResponse.RootElement.GetProperty("usage").GetProperty("prompt_tokens").ToString().Trim();
                var completionTokens = parsedResponse.RootElement.GetProperty("usage").GetProperty("completion_tokens").ToString().Trim();

                var responseContent = parsedResponse.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").ToString().Trim();

                generatedText = $"Response: {responseContent}\n\nUsage: prompt tokens - {promptTokens} & completion tokens - {completionTokens}\n\n{GetTotalCost(promptTokens, completionTokens)}";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating response: {ex.Message}");
        }

        return generatedText;
    }

    private static string GetTotalCost(string promptTokens, string completionTokens)
    {
        double promptCost = double.Parse(promptTokens) / 1000000 * 0.5;
        double completionCost = double.Parse(completionTokens) / 1000000 * 1.5;

        return $"Total estimated cost (USD): ${Math.Round(promptCost + completionCost, 5)}";
    }
}