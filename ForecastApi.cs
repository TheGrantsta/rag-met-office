public class ForecastApi
{
    public class ForecastApiParams(string latitude, string longitude, bool isMetOffice)
    {
        public string Latitude { get; set; } = latitude;
        public string Longitude { get; set; } = longitude;
        public bool GetIsMetOffice() => isMetOffice;

        public string GetApiKey() => GetIsMetOffice() ?
            Utils.GetConfigurationValues("MetOfficeApiKey") :
            Utils.GetConfigurationValues("TomorrowIoApiKey");

        public string GetApiUrl() => GetIsMetOffice() ?
            $"https://data.hub.api.metoffice.gov.uk/sitespecific/v0/point/hourly?latitude={Latitude}&longitude={Longitude}" :
            $"https://api.tomorrow.io/v4/weather/forecast?location={Latitude},{Longitude}";
    }

    public static async Task<string> Fetch(ForecastApiParams forecastApiParams)
    {
        var jsonResponse = string.Empty;

        try
        {
            using (var client = new HttpClient()) {
                client.DefaultRequestHeaders.Add("ApiKey", forecastApiParams.GetApiKey());

                var response = await client.GetAsync(forecastApiParams.GetApiUrl());
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
