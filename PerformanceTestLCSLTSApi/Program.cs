// Usage example
using PerformanceTestLCSLTSApi;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())            
            .AddUserSecrets<Program>();
        
        IConfiguration config = builder.Build();

        Settings settings = new Settings(config);


        var processor = new XmlRequestProcessor(
            apiUrl: settings.LcsCredentials.serviceurl + "/xml/AccommodationDataSearch",
            username: settings.LcsCredentials.username,
            password: settings.LcsCredentials.password,
            messagePassword: settings.LcsCredentials.messagepassword,
            newStartDate: "2025-12-01",  // Optional: set to null to keep original
            newEndDate: "2025-12-05"     // Optional: set to null to keep original
        );

        var xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\LCS");
        await processor.ProcessXmlFilesAsync(Path.GetFullPath(xmlPath));

        Console.WriteLine("Processing complete. Press any key to exit.");        


        using (var jprocessor = new JsonRequestProcessor(
            apiUrl: settings.LtsCredentials.serviceurl + "/accommodations/availabilities/search",
            username: settings.LtsCredentials.username,
            password: settings.LtsCredentials.password,
            clientId: settings.LtsCredentials.ltsclientid,
            newStartDate: "2025-12-01",  // Optional: set to null to keep original
            newEndDate: "2025-12-05"     // Optional: set to null to keep original
        ))
        {
            var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\LTS");
            await jprocessor.ProcessJsonFilesAsync(Path.GetFullPath(jsonPath));
        }

        Console.WriteLine("Processing complete. Press any key to exit.");
        Console.ReadKey();

    }
}