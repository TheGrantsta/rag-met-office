    public class ForecastApi
    {
    public static async Task<string> Fetch(string apiUrl, string latitude, string longitude)
        {
            var jsonResponse = string.Empty;

            try
            {
                var requestUrl = $"{apiUrl}?latitude={latitude}&longitude={longitude}";

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
    }
