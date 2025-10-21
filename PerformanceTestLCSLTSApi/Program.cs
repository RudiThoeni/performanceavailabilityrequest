// Usage example
using Microsoft.Extensions.Configuration;
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

        List<Tuple<string, string>> datestotest = new List<Tuple<string, string>>()
        {
            Tuple.Create("2025-12-01","2025-12-05"),
            Tuple.Create("2025-12-02","2025-12-03"),
            Tuple.Create("2025-11-20","2025-11-23"),
            Tuple.Create("2025-11-20","2025-11-23"),
            Tuple.Create("2025-11-27","2025-11-28"),
            Tuple.Create("2025-11-24","2025-11-30"),
            Tuple.Create("2025-11-19","2025-11-23"),            
        };

        List<List<Tuple<int, long>?>?> resultslts = new List<List<Tuple<int, long>?>?>();
        List<List<Tuple<int, long>?>?> resultslcs = new List<List<Tuple<int, long>?>?>();

        foreach (var datetuple in datestotest)
        {
            List<Tuple<int, long>?>? ltsresult = null;
            List<Tuple<int, long>?>? lcsresult = null;

            var processor = new XmlRequestProcessor(
                apiUrl: settings.LcsCredentials.serviceurl + "/xml/AccommodationDataSearch",
                username: settings.LcsCredentials.username,
                password: settings.LcsCredentials.password,
                messagePassword: settings.LcsCredentials.messagepassword,
                newStartDate: datetuple.Item1,  // Optional: set to null to keep original
                newEndDate: datetuple.Item2     // Optional: set to null to keep original
            );

            var xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\LCS");
            lcsresult = await processor.ProcessXmlFilesAsync(Path.GetFullPath(xmlPath));

            resultslcs.Add(lcsresult);

            using (var jprocessor = new JsonRequestProcessor(
                apiUrl: settings.LtsCredentials.serviceurl + "/accommodations/availabilities/search",
                username: settings.LtsCredentials.username,
                password: settings.LtsCredentials.password,
                clientId: settings.LtsCredentials.ltsclientid,
                newStartDate: datetuple.Item1,  // Optional: set to null to keep original
                newEndDate: datetuple.Item2     // Optional: set to null to keep original
            ))
            {
                var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\LTS");
                ltsresult = await jprocessor.ProcessJsonFilesAsync(Path.GetFullPath(jsonPath));

                resultslts.Add(ltsresult);
            }
        }


        Console.WriteLine("Comparision");

        for (int i = 0; i < 7; i++)
        {
            var lcsresultround = resultslcs[i];
            var ltsresultround = resultslts[i];

            for (int j = 0; j < 5; j++)
            {
                Console.Write("Round; {0}/{1} ; ", i, j);
                Console.WriteLine("LCS; {0} ; {1} ; LTS ; {2} ; {3}", lcsresultround[j].Item2, lcsresultround[j].Item1, ltsresultround[j].Item2, ltsresultround[j].Item1);
            }
        }


        Console.WriteLine("Processing complete. Press any key to exit.");
        Console.ReadKey();

    }
}