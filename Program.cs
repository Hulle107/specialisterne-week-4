using System.Threading;

namespace PDFDownloader;

internal class Program
{
    private static readonly ExcelReader ExcelReader = new();
    private static readonly Downloader Downloader = new();
    private static readonly Storage Storage = new();

    public static List<DownloadEntry> Entries = [];
    public static readonly int MaxConcurrentDownloads = 5;
    public static bool IsRunning = true;

    public static readonly List<Task> Runners = [];

    public static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
    public static readonly string LogDirectory = BaseDirectory;
    public static readonly string DownloadDirectory = Path.Combine(BaseDirectory, "downloads");
    public static readonly string? NASDirectory;
    
    public static readonly string? NASUsername;
    public static readonly string? NASPassword;

    private static void Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _ = Task.Run(() =>
        {
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                IsRunning = false;
            };
            cts.Cancel();
        });
        Thread tui = new(TUI);
        tui.Start();

        Entries = ExcelReader.Read();

        try
        {
            for (int poolId = 0; poolId < Entries.Count; poolId += MaxConcurrentDownloads)
            {
                for (int runnerId = poolId; runnerId < poolId + MaxConcurrentDownloads && runnerId < Entries.Count; runnerId++)
                {
                    var entry = Entries[runnerId];
                    Runners.Add(Runner(entry, token));
                }

                Task.WaitAll([.. Runners]);
                Runners.Clear();
            }

            IsRunning = false;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was cancelled.");
        }
    }

    private static void TUI()
    {
        while(IsRunning)
        {

        }
    }

    private static async Task Runner(DownloadEntry entry, CancellationToken cancellationToken)
    {
        try
        {
            var result = await Downloader.Download(entry, cancellationToken);

            if (result.Data is not null)
            {
                await Storage.StoreLocal(entry, result.Data, cancellationToken);
                await Storage.StoreNAS(entry, result.Data, cancellationToken);
            }
        }
        catch (Exception exception)
        {
            Logger.Log(new Exception($"{entry.BRNumber}", exception));
        }
    }
}