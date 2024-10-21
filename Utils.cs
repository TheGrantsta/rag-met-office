using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

public class Utils
{
    public static string GetConfigurationValues(string keyName)
    {
        var builder = new ConfigurationBuilder().AddUserSecrets<Program>();

        IConfiguration configuration = builder.Build();

        return configuration[keyName] ?? "";
    }

    public static string Boo(int weatherCode)
    {
        Dictionary<int, string> weathers = new Dictionary<int, string>
        {
            { 0, "clear night" },
            { 1, "sunny day" },
            { 2, "partly cloudy night" },
            { 3, "sunny intervals" },
            { 4, "overcast" }, //?
            { 5, "cloudy" }, //?
            { 6, "overcast" },
            { 7, "cloudy" },
            { 8, "overcast" },
            { 9, "light shower night" },
            { 10, "overcast" }, //?
        };

        if (!weathers.TryGetValue(weatherCode, out string? weather))
        {
            weather = "unknown";
        };

        return weather;
    }
}
