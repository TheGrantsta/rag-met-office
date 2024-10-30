public class ForecastApi
{
    public struct ForecastApiParams(string apiUrl, string latitude, string longitude, string apiKey)
    {
        public string ApiUrl { get; set; } = apiUrl;
        public string Latitude { get; set; } = latitude;
        public string Longitude { get; set; } = longitude;
        public string ApiKey { get; set; } = apiKey;
    }

    public static async Task<string> Fetch( ForecastApiParams forecastApiParams)// string apiUrl, string latitude, string longitude, string apiKey)
    {
        var jsonResponse = string.Empty;

        try
        {
            var requestUrl = $"{forecastApiParams.ApiUrl}?latitude={forecastApiParams.Latitude}&longitude={forecastApiParams.Longitude}";

            using (var client = new HttpClient()) {
                client.DefaultRequestHeaders.Add("ApiKey", forecastApiParams.ApiKey);

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
