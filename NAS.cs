using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PDFDownloader;

internal class NAS
{
    public static string? NASDirectory = null;
    public static string? NASUsername = null;
    public static string? NASPassword = null;

    public static bool UseNAS => !string.IsNullOrEmpty(NASDirectory) && !string.IsNullOrEmpty(NASUsername) && !string.IsNullOrEmpty(NASPassword);

    // P/Invoke for connecting to remote share
    [DllImport("mpr.dll")]
    private static extern int WNetAddConnection2(ref NetResource netResource, string password, string username, int flags);

    // P/Invoke for disconnecting from remote share
    [DllImport("mpr.dll")]
    private static extern int WNetCancelConnection2(string name, int flags, bool force);

    // Structure for network resource
    [StructLayout(LayoutKind.Sequential)]
    public struct NetResource
    {
        public int Scope;
        public int ResourceType;
        public int DisplayType;
        public int Usage;
        public string LocalName;
        public string RemoteName;
        public string Comment;
        public string Provider;
    }

    public static void UploadFile(Entry entry)
    {
        // Skip if NAS is not configured
        if (!UseNAS)
        {
            entry.UploadState = UploadState.Skipped;
            return;
        }

        string filename = $"{entry.Id}.pdf"; // Assuming PDF files; adjust as necessary
        string filePath = Path.Combine(entry.UsedOutputFilePath!, filename); // Full path to the downloaded file

        // Skip if file does not exist
        if (!File.Exists(filePath))
        {
            entry.UploadState = UploadState.Skipped;
            return;
        }

        string nasDirectory = NASDirectory!.Replace("/", "\\"); // Ensure correct path format
        string nasFilePath = Path.Combine(nasDirectory, filename); // Full path on NAS

        entry.UploadState = UploadState.Uploading;

        // Setup network resource
        NetResource netResource = new()
        {
            Scope = 0,
            ResourceType = 1, // Disk
            DisplayType = 0,
            Usage = 0,
            LocalName = null,
            RemoteName = nasDirectory,
            Comment = null,
            Provider = null
        };

        try
        {
            int result = WNetAddConnection2(ref netResource, NASPassword!, NASUsername!, 0); // Connect to NAS
            if (result != 0)
            {
                entry.UploadState = UploadState.Failed;
                _ = Logger.Log($"Failed to connect to NAS: Error {result}"); // Log connection error
                return;
            }

            File.Copy(filePath, nasFilePath, true); // Copy file to NAS
            entry.UploadState = UploadState.Completed;
        }
        catch (Exception exception)
        {
            entry.UploadState = UploadState.Failed;
            // Log any exceptions during file upload
            _ = Logger.Log($"[{entry.Id}] Failed to upload file to NAS: {exception.Message}");
        }
        finally
        {
            // Disconnect from NAS
            _ = WNetCancelConnection2(nasDirectory, 0, true);
        }
    }
}
