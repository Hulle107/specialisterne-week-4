using System.Collections.Concurrent;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PDFDownloader;

internal class Logger
{
    public static string LogDirectory { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

    private static Task? LogWriterTask;
    private static string LogFilePath = Path.Combine(LogDirectory, $"{AppDomain.CurrentDomain.FriendlyName}_{DateTime.Now:yyyyMMdd}.log");
    private static readonly BlockingCollection<string> LogQueue = [];

    public static void Setup()
    {
        if (!Directory.Exists(LogDirectory)) Directory.CreateDirectory(LogDirectory); // Ensure log directory exists
        LogFilePath = Path.Combine(LogDirectory, $"{AppDomain.CurrentDomain.FriendlyName}_{DateTime.Now:yyyyMMdd}.log"); // Set log file path
    }

    public static void Start(CancellationToken cancellationToken)
    {
        if (LogWriterTask is not null) return; // Already started
        LogWriterTask = Task.Run(() => ProcessLogQueue(cancellationToken), cancellationToken); // Start log processing task
    }

    public static async Task Log(string message)
    {
        LogQueue.Add(message); // Enqueue log message
    }

    private static void ProcessLogQueue(CancellationToken cancellationToken)
    {
        try
        {
            using StreamWriter writer = new(LogFilePath, append: true); // Append to existing log file

            foreach (var logEntry in LogQueue.GetConsumingEnumerable(cancellationToken))
            {
                writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {logEntry}"); // Timestamp each log entry
                writer.Flush(); // Ensure the log entry is written immediately
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Logging error: {exception.Message}"); // Log to console if file writing fails
        }
    }
}
