using Microsoft.Extensions.Configuration;

public class Utils
{
    public static string GetConfigurationValues(string keyName)
    {
        var builder = new ConfigurationBuilder().AddUserSecrets<Program>();

        IConfiguration configuration = builder.Build();

        return configuration[keyName] ?? "";
    }
}
