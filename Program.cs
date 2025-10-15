using CommandLine;

namespace PDFDownloader;

internal class Program
{
    public static string InputFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "input.csv");
    public static int ConcurrentDownloads = 5;

    public class Options
    {
        [Option('f', "file-path", Required = true, HelpText = "Path to the file containing download entries.")]
        public required string FilePath { get; set; }
        [Option('c', "concurrent-downloads", Required = false, HelpText = "Maximum number of concurrent downloads.")]
        public int? ConcurrentDownloads { get; set; }

        [Option('l', "log-directory", Required = false, HelpText = "Path to the directory where log files will be stored.")]
        public string? LogDirectory { get; set; }
        [Option('d', "download-directory", Required = false, HelpText = "Path to the directory where downloaded files will be stored.")]
        public string? DownloadDirectory { get; set; }
        [Option('o', "output-file-path", Required = false, HelpText = "Path to the output file where results will be saved.")]
        public string? OutputFilePath { get; set; }

        [Option('n', "nas-directory", Required = false, HelpText = "Path to the Excel file containing download entries.")]
        public string? NASDirectory { get; set; }
        [Option('u', "nas-username", Required = false, HelpText = "Username for NAS authentication.")]
        public string? NASUsername { get; set; }
        [Option('p', "nas-password", Required = false, HelpText = "Password for NAS authentication.")]
        public string? NASPassword { get; set; }
    }

    private static void Main(string[] args)
    {
        // Setup cancellation token
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Parse command-line arguments
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(options =>
            {
                InputFilePath = options.FilePath;
                if (options.ConcurrentDownloads is not null) ConcurrentDownloads = (int)options.ConcurrentDownloads;

                if (options.DownloadDirectory is not null) Downloader.DownloadDirectory = options.DownloadDirectory;
                if (options.LogDirectory is not null) Logger.LogDirectory = options.LogDirectory;
                if (options.OutputFilePath is not null) Output.OutputFilePath = options.OutputFilePath;

                NAS.NASDirectory = options.NASDirectory;
                NAS.NASUsername = options.NASUsername;
                NAS.NASPassword = options.NASPassword;
            })
            .WithNotParsed(errs => {
                foreach (var err in errs) Console.WriteLine(err.ToString());
                Environment.Exit(1);
            });

        Downloader.Setup(); // Setup downloader
        Output.Setup(); // Setup output
        Logger.Setup(); // Setup logger

        // Validate input file
        if (!File.Exists(InputFilePath))
        {
            Console.WriteLine($"Input file not found: {InputFilePath}");
            Environment.Exit(1);
        }

        Logger.Start(token); // Start the logger

        string extension = Path.GetExtension(InputFilePath); // Get the file extension
        List<Entry> entries = []; // Initialize the list of entries

        // Read entries from the input file based on its extension
        if (extension == ".xls" || extension == ".xlsx") entries = Reader.ReadExcelFile(InputFilePath); // Read Excel file
        if (extension == ".csv") entries = Reader.ReadCRVFile(InputFilePath); // Read CSV file

        if (entries.Count == 0)
        {
            Console.WriteLine($"File does not contain any entries.");
            Environment.Exit(1);
        }

        // Process entries in batches for concurrent downloads
        for (int cuncurrentIndex = 0; cuncurrentIndex < entries.Count; cuncurrentIndex+=ConcurrentDownloads)
        {
            var tasks = new List<Task>(); // List to hold download tasks

            for (int taskIndex = 0; taskIndex < ConcurrentDownloads; taskIndex++)
            {
                int entryIndex = cuncurrentIndex + taskIndex; // Calculate the actual index of the entry
                if (entryIndex >= entries.Count) break; // Break if index exceeds the number of entries

                var entry = entries[entryIndex]; // Get the current entry
                var task = Task.Run(async () => // Create a new task
                {
                    await Downloader.DownloadFileAsync(entry, token); // Download the file
                    NAS.UploadFile(entry); // Upload to NAS if configured
                }, token);

                tasks.Add(task); // Add the task to the list
            }

            Task.WaitAll([.. tasks]); // Wait for all tasks in the current batch to complete
            Output.SaveMetadata(entries); // Save metadata after each batch
        }
    }
}