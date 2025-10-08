namespace PDFDownloader;

enum DownloadStatus
{
    Pending,
    Downloading,
    Completed,
    Failed
}

enum StoreStatus
{
    Pending,
    Storing,
    Completed,
    Failed
}

internal class DownloadEntry
{
    public required string BRNumber { get; set; }
    public string? PrimaryUrl { get; set; }
    public string? AlternateUrl { get; set; }

    public bool IsValid() => PrimaryUrl is not null || AlternateUrl is not null;

    public DownloadStatus PrimaryDownloadStatus { get; set; } = DownloadStatus.Pending;
    public DownloadStatus AlternateDownloadStatus { get; set; } = DownloadStatus.Pending;
    public StoreStatus LocalStoreStatus { get; set; } = StoreStatus.Pending;
    public StoreStatus NASStoreStatus { get; set; } = StoreStatus.Pending;
}
