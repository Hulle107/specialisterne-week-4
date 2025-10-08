using System.Runtime.InteropServices;
using System.Threading;

namespace PDFDownloader;

internal class Storage
{
    [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
    private static extern int WNetAddConnection2(ref NETRESOURCE lpNetResource,
                                                 string lpPassword,
                                                 string lpUsername,
                                                 int dwFlags);
    [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
    private static extern int WNetCancelConnection2(string lpName, int dwFlags, bool force);
    [StructLayout(LayoutKind.Sequential)]
    private struct NETRESOURCE
    {
        public int dwScope;
        public int dwType;
        public int dwDisplayType;
        public int dwUsage;
        public string lpLocalName;
        public string lpRemoteName;
        public string lpComment;
        public string lpProvider;
    }

    public async Task StoreLocal(DownloadEntry entry, byte[] data, CancellationToken cancellationToken)
    {
        entry.LocalStoreStatus = StoreStatus.Storing;

        var directory = Program.DownloadDirectory;
        var filename = entry.BRNumber;
        var fullPath = Path.Combine(directory, $"{filename}.pdf");

        var directoryExists = Directory.Exists(directory);
        if (!directoryExists) Directory.CreateDirectory(directory);

        var fileExists = File.Exists(fullPath);

        if (fileExists)
        {
            entry.LocalStoreStatus = StoreStatus.Failed;
            throw new Exception($"File {entry.BRNumber}.pdf already exists in local download directory.");
        }

        try
        {
            await File.WriteAllBytesAsync(fullPath, data, cancellationToken);
            entry.LocalStoreStatus = StoreStatus.Completed;
        }
        catch (Exception)
        {
            entry.LocalStoreStatus = StoreStatus.Failed;
            throw;
        }
    }

    public async Task StoreNAS(DownloadEntry entry, byte[] data, CancellationToken cancellationToken)
    {
        entry.LocalStoreStatus = StoreStatus.Storing;

        var directory = Program.DownloadDirectory;
        var filename = entry.BRNumber;
        var fullPath = Path.Combine(directory, $"{filename}.pdf");

        if (string.IsNullOrEmpty(directory))
        {
            entry.NASStoreStatus = StoreStatus.Failed;
            throw new Exception("NAS directory is not configured.");
        }

        if (string.IsNullOrEmpty(Program.NASUsername) || string.IsNullOrEmpty(Program.NASPassword))
        {
            entry.NASStoreStatus = StoreStatus.Failed;
            throw new Exception("NAS credentials are not configured.");
        }

        var resource = new NETRESOURCE
        {
            dwType = 1,
            lpRemoteName = directory
        };

        int result = WNetAddConnection2(ref resource, Program.NASPassword, Program.NASUsername, 0);

        if (result == 0)
        {
            var directoryExists = Directory.Exists(directory);

            if (!directoryExists)
            {
                entry.NASStoreStatus = StoreStatus.Failed;
                throw new Exception($"NAS directory {directory} does not exist.");
            }

            var fileExists = File.Exists(fullPath);

            if (fileExists)
            {
                entry.NASStoreStatus = StoreStatus.Failed;
                throw new Exception($"File {filename}.pdf already exists on NAS.");
            }

            await File.WriteAllBytesAsync(fullPath, data, cancellationToken);
            entry.NASStoreStatus = StoreStatus.Completed;
            _ = WNetCancelConnection2(directory, 0, true);
        }

        entry.NASStoreStatus = StoreStatus.Failed;
        throw new Exception($"Failed to connect to NAS. Error code: {result}");
    }
}
