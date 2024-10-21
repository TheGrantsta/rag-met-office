﻿class Program()
{
    public struct Coordinates(string latitude, string longitude)
    {
        public string Latitude { get; set; } = latitude;
        public string Longitude { get; set; } = longitude;
    }

    private static Coordinates _coordinates = new("51.652931", "-0.199610");

    static async Task Main()
    {
        var hourlySpotData = await WeatherApi.FetchDataFromApi("https://data.hub.api.metoffice.gov.uk/sitespecific/v0/point/hourly", _coordinates.Latitude, _coordinates.Longitude);

        var hourlySpotDataAsText = Utils.ExtractTextFromJson(hourlySpotData);

        if(hourlySpotDataAsText.Count > 0)
        {
            var generatedResponse = await GenerateResponseBasedOnContext(hourlySpotDataAsText);

            Console.WriteLine($"\n{generatedResponse}\n");
        }
    }

       static async Task<string> GenerateResponseBasedOnContext(List<string> strings)
    {
        // For simplicity, we are just going to concatenate the weather statements into a simple context for now.
        var context = string.Join("\n", strings.Select(e => string.Join(". ", e)));

        return await OpenAiApi.GenerateResponseBasedOnContext(context,_coordinates.Latitude, _coordinates.Longitude);
    }
}
