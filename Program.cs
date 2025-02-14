using static ForecastApi;

class Program()
{
    private static ForecastApiParams? _forecastApiParams;
    static async Task Main(string[] args)
    {
        Console.WriteLine("Enter latitude: (hint 51.652931)");
        var latitudeInput = Console.ReadLine();

        Console.WriteLine("Enter longitude: (hint -0.199610)");
        var longitudeInput = Console.ReadLine();

        string latitude = string.IsNullOrWhiteSpace(latitudeInput) ? "51.652931" : latitudeInput;
        string longitude = string.IsNullOrWhiteSpace(longitudeInput) ? "-0.199610" : longitudeInput;

        _forecastApiParams = new ForecastApiParams(IsMetOffice(args), latitude, longitude);

        var forecastDataAsText = Utils.ExtractTextFromJson((string?)await Fetch(_forecastApiParams), _forecastApiParams.GetIsMetOffice());

        if (forecastDataAsText.Count > 0)
        {
            Console.WriteLine($"\n{await GenerateResponseBasedOnContext(forecastDataAsText)}\n");
        }
    }

    static bool IsMetOffice(string[] args)
    {
        return (args.Length == 0) || args.First().Equals("MetOffice");
    }

    static async Task<string> GenerateResponseBasedOnContext(List<string> strings)
    {
        // For simplicity, we are just going to concatenate the weather statements into a simple context for now.
        var context = string.Join("\n", strings.Select(e => string.Join(". ", e)));

        return await OpenAiApi.GenerateResponseBasedOnContext(_forecastApiParams, context);
    }
}
