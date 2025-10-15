using OfficeOpenXml;

namespace PDFDownloader;

internal class Reader
{
    public static List<Entry> ReadExcelFile(string filepath)
    {
        List<Entry> entries = []; // Initialize the list of entries

        try
        {
            using var package = new ExcelPackage(new FileInfo(filepath)); // Load the Excel file
            var worksheet = package.Workbook.Worksheets[0]; // Get the first worksheet

            int rowCount = worksheet.Dimension.Rows; // Get the number of rows
            for (int row = 2; row < rowCount; row++)
            {
                // Assuming ID is in the first column (A) and URLs are in the 38th (AL) and 39th (AM) columns
                entries.Add(new Entry()
                {
                    Id = worksheet.Cells[row, 1].ToString(),
                    Urls = [
                        worksheet.Cells[row, 38].ToString(),
                        worksheet.Cells[row, 39].ToString(),
                    ],
                });
            }
        }
        catch (Exception)
        {
            // Log the error if reading the Excel file fails
            _ = Logger.Log($"Failed to read Excel file: {filepath}");
        }

        return entries;
    }

    public static List<Entry> ReadCRVFile(string filepath)
    {
        List<Entry> entries = []; // Initialize the list of entries

        try
        {
            var lines = File.ReadAllLines(filepath); // Read all lines from the CSV file

            // Skip the header line and process each subsequent line
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(','); // Split the line by commas

                if (parts.Length < 39) continue; // Ensure there are enough parts

                // Assuming ID is in the first column and URLs are in the 38th and 39th columns
                entries.Add(new Entry()
                {
                    Id = parts[0],
                    Urls = [
                        parts[37],
                        parts[38],
                    ],
                });
            }
        }
        catch (Exception)
        {
            // Log the error if reading the CSV file fails
            _ = Logger.Log($"Failed to read CSV file: {filepath}");
        }

        return entries;
    }
}
