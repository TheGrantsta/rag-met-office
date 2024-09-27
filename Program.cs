using System;
using Microsoft.Extensions.Configuration;

class Program()
{
    private static string _MetOfficeApiKey = String.Empty;
    static async Task Main(string[] args)
    {
        GetConfigurationValues();

        Console.WriteLine("Finished!");
    }

    private static void GetConfigurationValues()
    {
        var builder = new ConfigurationBuilder().AddUserSecrets<Program>();

        IConfiguration configuration = builder.Build();

        _MetOfficeApiKey = configuration["MetOfficeApiKey"]??"";
    }
}
