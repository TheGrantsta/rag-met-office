using static ForecastApi;

class Program()
{
    private static ForecastApiParams _forecastApiParams;
    static async Task Main(string[] args)
    {
        var source = (args.Length == 0) ? "MetOffice" : args.First();
        var isMetOffice = source.Equals("MetOffice");

        Console.WriteLine($"Source: {source}.");

        Console.WriteLine("Enter latitude: (hint 51.652931)");
        var latitude = Console.ReadLine();

        Console.WriteLine("Enter longitude: (hint -0.199610)");
        var longitude = Console.ReadLine();

        var apiUrl = isMetOffice ? "https://data.hub.api.metoffice.gov.uk/sitespecific/v0/point/hourly?" : "https://api.tomorrow.io/v4/weather/forecast?location=";

        var apiKey = isMetOffice ? Utils.GetConfigurationValues("MetOfficeApiKey") : Utils.GetConfigurationValues("TomorrowIoApiKey");

        _forecastApiParams = new ForecastApiParams(apiUrl, latitude, longitude, apiKey);

        var forecastData = await Fetch(_forecastApiParams);

        var forecastDataAsText = Utils.ExtractTextFromJson(forecastData, isMetOffice);

        if(forecastDataAsText.Count > 0)
        {
            var generatedResponse = await GenerateResponseBasedOnContext(forecastDataAsText);

            Console.WriteLine($"\n{generatedResponse}\n");
        }
    }

       static async Task<string> GenerateResponseBasedOnContext(List<string> strings)
    {
        // For simplicity, we are just going to concatenate the weather statements into a simple context for now.
        var context = string.Join("\n", strings.Select(e => string.Join(". ", e)));

        return await OpenAiApi.GenerateResponseBasedOnContext(context, _forecastApiParams.Latitude, _forecastApiParams.Longitude);
    }
}
