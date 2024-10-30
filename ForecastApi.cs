public class ForecastApi
{
    public struct ForecastApiParams(string apiUrl, string latitude, string longitude, string apiKey)
    {
        public string ApiUrl { get; set; } = apiUrl;
        public string Latitude { get; set; } = latitude;
        public string Longitude { get; set; } = longitude;
        public string ApiKey { get; set; } = apiKey;
    }

    public static async Task<string> Fetch( ForecastApiParams forecastApiParams)
    {
        var jsonResponse = string.Empty;

        try
        {
            using (var client = new HttpClient()) {
                client.DefaultRequestHeaders.Add("ApiKey", forecastApiParams.ApiKey);

                var response = await client.GetAsync(forecastApiParams.ApiUrl);
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
