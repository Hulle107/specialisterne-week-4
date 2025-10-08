using System.Net.Http.Headers;

namespace PDFDownloader;

internal class DownloadResult
{
    public byte[]? Data { get; set; }
}

internal class Downloader
{

    public async Task<DownloadResult> Download(DownloadEntry entry, CancellationToken cancellationToken)
    {
        if (entry.PrimaryUrl is not null)
        {
            entry.PrimaryDownloadStatus = DownloadStatus.Downloading;

            var result = await DownloadFromURL(entry, cancellationToken);

            if (result.Data is not null)
            {
                entry.PrimaryDownloadStatus = DownloadStatus.Completed;

                return result;
            }

            entry.PrimaryDownloadStatus = DownloadStatus.Failed;
        }

        if (entry.AlternateUrl is not null)
        {
            entry.AlternateDownloadStatus = DownloadStatus.Downloading;

            var result = await DownloadFromURL(entry, cancellationToken);

            if (result.Data is not null)
            {
                entry.AlternateDownloadStatus = DownloadStatus.Completed;

                return result;
            }

            entry.AlternateDownloadStatus = DownloadStatus.Failed;
        }

        return new();
    }

    private async Task<DownloadResult> DownloadFromURL(DownloadEntry entry, CancellationToken cancellationToken)
    {
        using HttpClient client = new();

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/pdf"));

        var response = await client.GetAsync(entry.PrimaryUrl, cancellationToken);

        if (response.IsSuccessStatusCode && response.Content.Headers.ContentType?.MediaType == "application/pdf")
            return new()
            {
                Data = await response.Content.ReadAsByteArrayAsync(cancellationToken)
            };

        return new();
    }
}
