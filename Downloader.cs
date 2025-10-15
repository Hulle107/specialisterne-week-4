using System.Net.Http.Headers;

namespace PDFDownloader;

internal class Downloader
{
    public static string DownloadDirectory { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "downloads");

    public static void Setup()
    {
        if (!Directory.Exists(DownloadDirectory)) Directory.CreateDirectory(DownloadDirectory); // Create download directory if it doesn't exist
    }

    public static async Task DownloadFileAsync(Entry entry, CancellationToken cancellationToken)
    {
        // If there are no URLs, skip the entry
        if (entry.Urls.Count == 0)
        {
            entry.DownloadState = DownloadState.Skipped;
            return;
        }

        string filename = $"{entry.Id}.pdf"; // Use entry ID as filename
        string filePath = Path.Combine(DownloadDirectory, filename); // Full path to the file

        // If the file already exists, mark as completed and return
        if (File.Exists(filePath))
        {
            entry.UsedOutputFilePath = filePath;
            entry.DownloadState = DownloadState.Completed;
            return;
        }

        using HttpClient client = new(); // Create a new HttpClient instance
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/pdf")); // Set Accept header to application/pdf

        entry.DownloadState = DownloadState.Downloading;

        // Try each URL until one succeeds
        foreach (var url in entry.Urls)
        {
            try
            {
                // Send GET request
                var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode(); // Throw if not a success code

                // Save the response content to a file
                using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken),
                    fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                await stream.CopyToAsync(fileStream, cancellationToken); // Copy the content to the file
                await fileStream.FlushAsync(cancellationToken); // Ensure all data is written to the file

                // Update entry details
                entry.UsedUrl = url;
                entry.UsedOutputFilePath = filePath;
                entry.DownloadState = DownloadState.Completed;

                return;
            }
            catch (Exception ex)
            {
                // Log the failure and try the next URL
                await Logger.Log($"[{entry.Id}] Failed to download {entry.Id} from {url}: {ex.Message}");
            }
        }

        // If all URLs failed, mark the entry as failed
        await Logger.Log($"[{entry.Id}] All download attempts failed for {entry.Id}");
        entry.DownloadState = DownloadState.Failed;
    }
}
