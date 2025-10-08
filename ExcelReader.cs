using ClosedXML.Excel;

namespace PDFDownloader;

internal class ExcelReader
{
    public int BRColumnIndex = 1; // Column A
    public int PrimaryUrlColumnIndex = 38; // Column AL
    public int AlternateUrlColumnIndex = 39; // Column AM

    public List<DownloadEntry> Read(string filepath)
    {
        List<DownloadEntry> entries = [];

        using (var workbook = new XLWorkbook(filepath))
        {
            var worksheet = workbook.Worksheets.First();

            foreach (var row in worksheet.RowsUsed())
            {
                var br = row.Cell(BRColumnIndex).ToString()!;
                var primaryUrl = row.Cell(PrimaryUrlColumnIndex).ToString();
                var alternateUrl = row.Cell(AlternateUrlColumnIndex).ToString();

                DownloadEntry entry = new()
                {
                    BRNumber = br,
                    PrimaryUrl = primaryUrl,
                    AlternateUrl = alternateUrl
                };

                entries.Add(entry);
            }
        }
        
        return entries;
    }
}
