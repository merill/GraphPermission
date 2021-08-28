using CsvHelper;
using GraphMarkdown.Data;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace GraphMarkdown.Infrastructure
{
    public static class Csv
    {
        public static void SavePermissionsToCsv(List<DocGraphPermission> permissions, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(permissions);
        }
    }
}
