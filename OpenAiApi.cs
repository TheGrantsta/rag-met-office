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
            "You are an experienced meteorologist at the Met Office. Please provide your professional forecast for the next 4 hours. The response should target a reading grade of Year 4; avoid polysyllabic words. Keep your answer short-summarise in short sentences, no more than 3 sentences and a maximum of 30 words. Round all temperatures to the nearest whole number. Ensure accuracy without fabricating information—fact-check internally, but do not include those checks in your response." :
            "You are an experienced meteorologist at the Met Office. Please tell me what the weather will be like in the next hour. Say if it will start or stop raining, and when (in minutes). The response should target a reading grade of Year 4; avoid polysyllabic words. Keep your answer short—summarise in short sentences, no more than 3 sentences and a maximum of 30 words. Round temperatures to whole numbers. Make sure it's correct, but don’t show how you checked it.";

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