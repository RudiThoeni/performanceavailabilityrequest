
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Collections.Generic;

namespace PerformanceTestLCSLTSApi
{
    public class XmlRequestProcessor
    {
        private readonly string _apiUrl;
        private readonly string _username;
        private readonly string _password;
        private readonly string _messagePassword;
        private readonly string _newStartDate;
        private readonly string _newEndDate;

        public XmlRequestProcessor(
            string apiUrl,
            string username,
            string password,
            string messagePassword,
            string newStartDate = null,
            string newEndDate = null)
        {
            _apiUrl = apiUrl;
            _username = username;
            _password = password;
            _messagePassword = messagePassword;
            _newStartDate = newStartDate;
            _newEndDate = newEndDate;
        }

        public async Task<List<Tuple<int, long>?>?> ProcessXmlFilesAsync(string directoryPath)
        {
            List<Tuple<int, long>> resultlist = new List<Tuple<int, long>>();

            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Directory not found: {directoryPath}");
                return null;
            }

            var xmlFiles = Directory.GetFiles(directoryPath, "*.xml");

            if (xmlFiles.Length == 0)
            {
                Console.WriteLine("No XML files found in the directory.");
                return null;
            }

            Console.WriteLine($"Found {xmlFiles.Length} XML file(s) to process.\n");

            foreach (var filePath in xmlFiles)
            {
                var result = await ProcessSingleFileAsync(filePath);
                resultlist.Add(result);
            }


        }

        private async Task<Tuple<int, long>?> ProcessSingleFileAsync(string filePath)
        {
            try
            {
                Console.WriteLine($"Processing file: {Path.GetFileName(filePath)}");

                // Load XML
                var xmlDoc = XDocument.Load(filePath);

                // Modify XML - Set MessagePassword
                var requestorId = xmlDoc.Descendants("RequestorID").FirstOrDefault();
                if (requestorId != null)
                {
                    requestorId.SetAttributeValue("MessagePassword", _messagePassword);
                    Console.WriteLine($"  Set MessagePassword to: {_messagePassword}");
                }
                else
                {
                    Console.WriteLine("  Warning: RequestorID element not found");
                }

                // Modify XML - Update TimeSpan dates if provided
                var timeSpan = xmlDoc.Descendants("TimeSpan").FirstOrDefault();
                if (timeSpan != null)
                {
                    if (!string.IsNullOrEmpty(_newStartDate))
                    {
                        timeSpan.SetAttributeValue("Start", _newStartDate);
                        Console.WriteLine($"  Set Start date to: {_newStartDate}");
                    }

                    if (!string.IsNullOrEmpty(_newEndDate))
                    {
                        timeSpan.SetAttributeValue("End", _newEndDate);
                        Console.WriteLine($"  Set End date to: {_newEndDate}");
                    }
                }
                else
                {
                    Console.WriteLine("  Warning: TimeSpan element not found");
                }

                // Prepare request
                var xmlContent = xmlDoc.ToString();

                // Make POST request and measure time
                var stopwatch = Stopwatch.StartNew();
                var responseXml = await SendPostRequestAsync(xmlContent);
                stopwatch.Stop();

                Console.WriteLine($"  Request completed in: {stopwatch.ElapsedMilliseconds} ms");

                // Parse response and extract ResultsQty
                if (!string.IsNullOrEmpty(responseXml))
                {
                    var responseDoc = XDocument.Parse(responseXml);
                    var result = responseDoc.Descendants("Result").FirstOrDefault();

                    if (result != null)
                    {
                        var resultsQty = result.Attribute("ResultsQty")?.Value;
                        Console.WriteLine($"  ResultsQty: {resultsQty}");
                    }
                    else
                    {
                        Console.WriteLine("  Warning: Result element not found in response");
                    }
                }

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error processing file: {ex.Message}");
                Console.WriteLine();
            }
        }

        private async Task<string> SendPostRequestAsync(string xmlContent)
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            using (var httpClient = new HttpClient(handler))
            {
                // Set Basic Authentication
                var authToken = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{_username}:{_password}"));
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                // Add Accept-Encoding header (optional, but explicit)
                httpClient.DefaultRequestHeaders.AcceptEncoding.Add(
                    new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                httpClient.DefaultRequestHeaders.AcceptEncoding.Add(
                    new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));

                // Prepare content
                var content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");

                // Send POST request
                var response = await httpClient.PostAsync(_apiUrl, content);
                response.EnsureSuccessStatusCode();

                // Return response as string (automatically decompressed)
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}