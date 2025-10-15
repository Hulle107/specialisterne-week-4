namespace PDFDownloader;

internal class Output
{
    public static string OutputFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "metadata.csv");

    public static void Setup()
    {
        if (File.Exists(OutputFilePath)) File.Delete(OutputFilePath); // Delete existing output file
    }

    public static void SaveMetadata(List<Entry> entries)
    {
        using StreamWriter writer = new(OutputFilePath); // Open the output file for writing

        writer.WriteLine("BRNumber,Url,Output,Downloaded,Uploaded"); // Write the header

        // Write each entry's metadata
        foreach (var entry in entries)
        {
            writer.WriteLine($"{entry.Id},{entry.UsedUrl},{entry.UsedOutputFilePath},{entry.DownloadState},{entry.UploadState}");
        }
    }
}
