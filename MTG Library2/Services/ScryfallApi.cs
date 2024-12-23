using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MTG_Library2.Services;

public class ScryfallApi
{
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task FetchAndSaveScryfallDataAsync(LoadingWindow loadingWindow)
    {
        try
        {
            string bulkDataUrl = "https://api.scryfall.com/bulk-data";
            string dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            string savePath = Path.Combine(dataDirectory, "all_cards.json");

            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "MTGLibraryApp/1.0 (your-email@example.com)");
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                var response = await client.GetAsync(bulkDataUrl);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to fetch bulk data: {response.StatusCode}");
                }

                string content = await response.Content.ReadAsStringAsync();
                var bulkData = JObject.Parse(content);

                var allCardsEntry = bulkData["data"]
                    .FirstOrDefault(entry => (string)entry["type"] == "default_cards");

                if (allCardsEntry == null)
                {
                    throw new Exception("Could not find the 'all_cards' bulk data entry.");
                }

                string downloadUri = allCardsEntry["download_uri"].ToString();
                using (var downloadResponse = await client.GetAsync(downloadUri, HttpCompletionOption.ResponseHeadersRead))
                {
                    if (!downloadResponse.IsSuccessStatusCode)
                    {
                        throw new Exception($"Failed to download card data: {downloadResponse.StatusCode}");
                    }

                    var totalBytes = downloadResponse.Content.Headers.ContentLength ?? -1L;
                    long downloadedBytes = 0;

                    using (var stream = await downloadResponse.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            downloadedBytes += bytesRead;

                            // Update progress
                            double progress = totalBytes > 0 ? (double)downloadedBytes / totalBytes * 100 : 0;
                            string status = $"{downloadedBytes / 1024} KB of {totalBytes / 1024} KB downloaded...";
                            loadingWindow.Dispatcher.Invoke(() =>
                            {
                                loadingWindow.UpdateProgress(progress, status);
                            });
                        }
                    }

                    loadingWindow.Dispatcher.Invoke(() =>
                    {
                        loadingWindow.UpdateProgress(100, "Download complete!");
                    });

                    Console.WriteLine($"Card data successfully saved to {savePath}");
                }
            }
        }
        catch (Exception ex)
        {
            loadingWindow.Dispatcher.Invoke(() =>
            {
                loadingWindow.UpdateProgress(0, $"Error: {ex.Message}");
            });
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public class BulkDataResponse
    {
        public List<BulkData> Data { get; set; }
    }

    public class BulkData
    {
        public string Type { get; set; }
        public string DownloadUri { get; set; }
    }
}