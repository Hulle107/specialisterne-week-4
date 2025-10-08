using System.Diagnostics;
using System.Text;

namespace PDFDownloader
{
    internal class Logger
    {
        public static void Log(Exception exception)
        {
            var filename = $"{Process.GetCurrentProcess().ProcessName}_{DateTime.Now:yyyy_MM_dd}.log";
            var file = Path.Combine(Program.LogDirectory, filename);
            var date = DateTime.Now;
            var message = $"{date}: {exception.Message}";
            var innerMessage = exception.InnerException?.Message;

            if (!string.IsNullOrEmpty(innerMessage)) message += $" {innerMessage}";

            File.AppendAllText(file, $"{message}{Environment.NewLine}", Encoding.UTF8);
        }
    }
}
