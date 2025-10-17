using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PerformanceTestLCSLTSApi
{
    public class JsonRequestProcessor : IDisposable
    {
        private readonly string _apiUrl;
        private readonly string _username;
        private readonly string _password;
        private readonly string _clientId;
        private readonly string _newStartDate;
        private readonly string _newEndDate;
        private readonly HttpClient _httpClient;

        public JsonRequestProcessor(
            string apiUrl,
            string username,
            string password,
            string clientId,
            string newStartDate = null,
            string newEndDate = null)
        {
            _apiUrl = apiUrl;
            _username = username;
            _password = password;
            _clientId = clientId;
            _newStartDate = newStartDate;
            _newEndDate = newEndDate;

            // Initialize HttpClient with gzip support
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            _httpClient = new HttpClient(handler);

            // Set Basic Authentication
            var authToken = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_username}:{_password}"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

            // Set custom header
            _httpClient.DefaultRequestHeaders.Add("X-LTS-ClientID", _clientId);
        }

        public async Task ProcessJsonFilesAsync(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Directory not found: {directoryPath}");
                return;
            }

            var jsonFiles = Directory.GetFiles(directoryPath, "*.json");

            if (jsonFiles.Length == 0)
            {
                Console.WriteLine("No JSON files found in the directory.");
                return;
            }

            Console.WriteLine($"Found {jsonFiles.Length} JSON file(s) to process.\n");

            foreach (var filePath in jsonFiles)
            {
                await ProcessSingleFileAsync(filePath);
            }
        }

        private async Task<Tuple<int, long>?> ProcessSingleFileAsync(string filePath)
        {
            try
            {                
                Console.WriteLine($"Processing file: {Path.GetFileName(filePath)}");

                // Load JSON
                var jsonContent = await File.ReadAllTextAsync(filePath);
                var jsonDoc = JsonDocument.Parse(jsonContent);

                // Modify JSON - Update startDate and endDate
                using (var stream = new MemoryStream())
                {
                    using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false }))
                    {
                        ModifyJson(jsonDoc.RootElement, writer);
                    }

                    var modifiedJson = Encoding.UTF8.GetString(stream.ToArray());

                    // Make POST request and measure time
                    var stopwatch = Stopwatch.StartNew();
                    var responseJson = await SendPostRequestAsync(modifiedJson);
                    stopwatch.Stop();

                    Console.WriteLine($"  Request completed in: {stopwatch.ElapsedMilliseconds} ms");

                    // Parse response and extract resultsQuantity
                    if (!string.IsNullOrEmpty(responseJson))
                    {
                        var responseDoc = JsonDocument.Parse(responseJson);

                        if (responseDoc.RootElement.TryGetProperty("paging", out var paging))
                        {
                            if (paging.TryGetProperty("resultsQuantity", out var resultsQuantity))
                            {
                                Console.WriteLine($"  resultsQuantity: {resultsQuantity.GetInt32()}");

                                return Tuple.Create(resultsQuantity.GetInt32(), stopwatch.ElapsedMilliseconds);
                            }
                            else
                            {
                                Console.WriteLine("  Warning: resultsQuantity not found in response");
                            }
                        }
                        else
                        {
                            Console.WriteLine("  Warning: paging element not found in response");
                        }
                    }

                    Console.WriteLine();

                    return Tuple.Create(0, stopwatch.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error processing file: {ex.Message}");
                Console.WriteLine();

                return null;
            }
        }

        private void ModifyJson(JsonElement element, Utf8JsonWriter writer)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    writer.WriteStartObject();
                    foreach (var property in element.EnumerateObject())
                    {
                        writer.WritePropertyName(property.Name);

                        // Check if we're in parameters object and modify dates
                        if (property.Name == "parameters" && property.Value.ValueKind == JsonValueKind.Object)
                        {
                            writer.WriteStartObject();
                            foreach (var param in property.Value.EnumerateObject())
                            {
                                writer.WritePropertyName(param.Name);

                                if (param.Name == "startDate" && !string.IsNullOrEmpty(_newStartDate))
                                {
                                    writer.WriteStringValue(_newStartDate);
                                    Console.WriteLine($"  Set startDate to: {_newStartDate}");
                                }
                                else if (param.Name == "endDate" && !string.IsNullOrEmpty(_newEndDate))
                                {
                                    writer.WriteStringValue(_newEndDate);
                                    Console.WriteLine($"  Set endDate to: {_newEndDate}");
                                }
                                else
                                {
                                    ModifyJson(param.Value, writer);
                                }
                            }
                            writer.WriteEndObject();
                        }
                        else
                        {
                            ModifyJson(property.Value, writer);
                        }
                    }
                    writer.WriteEndObject();
                    break;

                case JsonValueKind.Array:
                    writer.WriteStartArray();
                    foreach (var item in element.EnumerateArray())
                    {
                        ModifyJson(item, writer);
                    }
                    writer.WriteEndArray();
                    break;

                case JsonValueKind.String:
                    writer.WriteStringValue(element.GetString());
                    break;

                case JsonValueKind.Number:
                    writer.WriteNumberValue(element.GetDouble());
                    break;

                case JsonValueKind.True:
                    writer.WriteBooleanValue(true);
                    break;

                case JsonValueKind.False:
                    writer.WriteBooleanValue(false);
                    break;

                case JsonValueKind.Null:
                    writer.WriteNullValue();
                    break;
            }
        }

        private async Task<string> SendPostRequestAsync(string jsonContent)
        {
            // Prepare content
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Send POST request
            var response = await _httpClient.PostAsync(_apiUrl, content);
            response.EnsureSuccessStatusCode();

            // Return response as string (automatically decompressed)
            return await response.Content.ReadAsStringAsync();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}