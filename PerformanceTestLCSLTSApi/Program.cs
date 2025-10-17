// Usage example
using PerformanceTestLCSLTSApi;

class Program
{
    static async Task Main(string[] args)
    {
        var processor = new XmlRequestProcessor(
            apiUrl: "https://lcs.lts.it/api/data.svc/xml/AccommodationDataSearch",
            username: "secret",
            password: "secret",
            messagePassword: "secret",
            newStartDate: "2025-12-01",  // Optional: set to null to keep original
            newEndDate: "2025-12-05"     // Optional: set to null to keep original
        );

        var xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\LCS");
        await processor.ProcessXmlFilesAsync(Path.GetFullPath(xmlPath));

        Console.WriteLine("Processing complete. Press any key to exit.");        


        using (var jprocessor = new JsonRequestProcessor(
            apiUrl: "https://api.example.com/search",
            username: "secret",
            password: "secret",
            clientId: "secret",
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