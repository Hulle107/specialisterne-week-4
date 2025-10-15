namespace PDFDownloader;

enum DownloadState
{
    Skipped,
    Pending,
    Downloading,
    Saving,
    Completed,
    Failed,
}

enum UploadState
{
    Skipped,
    Pending,
    Uploading,
    Completed,
    Failed,
}

internal class Entry
{
    public required string Id { get; set; }
    public required List<string> Urls { get; set; } = [];

    public DownloadState DownloadState { get; set; } = DownloadState.Pending;
    public UploadState UploadState { get; set; } = UploadState.Pending;

    public string? UsedUrl { get; set; }
    public string? UsedOutputFilePath { get; set; }
}
