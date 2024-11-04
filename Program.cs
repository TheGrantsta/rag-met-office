using static ForecastApi;

class Program()
{
    private static ForecastApiParams? _forecastApiParams;
    static async Task Main(string[] args)
    {
        Console.WriteLine("Enter latitude: (hint 51.652931)");
        var latitude = Console.ReadLine();

        Console.WriteLine("Enter longitude: (hint -0.199610)");
        var longitude = Console.ReadLine();

        _forecastApiParams = new ForecastApiParams(latitude, longitude, isMetOffice: (args.Length == 0) || args.First().Equals("MetOffice"));

        var forecastData = await Fetch(_forecastApiParams);

        var forecastDataAsText = Utils.ExtractTextFromJson(forecastData, _forecastApiParams.GetIsMetOffice());

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

        return await OpenAiApi.GenerateResponseBasedOnContext(_forecastApiParams, context);
    }
}
