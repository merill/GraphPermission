using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace GraphPermissionParser
{
    class CsvHelper
    {
        public static void SavePermissionsToCsv(List<GraphPermission> permissions, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(permissions);
        }
    }
}
