using CsvHelper;
using GraphMarkdown.Data;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace GraphMarkdown.Infrastructure
{
    public static class Csv
    {
        public static void SavePermissionsToCsv(Dictionary<string, GraphPermissionMap> permMap, string filePath)
        {
            //using var writer = new StreamWriter(filePath);
            //using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            //csv.WriteRecord(permissions);

            using (StreamWriter outputFile = new StreamWriter(filePath))
            using (var wrt = new CsvWriter(outputFile, CultureInfo.CurrentCulture))
            {
                wrt.WriteField("PermissionName");
                wrt.WriteField("ApplicationPermission");
                wrt.WriteField("DelegatePermission");
                wrt.WriteField("SourceFiles");
                wrt.NextRecord();

                foreach (var perm in permMap)
                {
                    var item = perm.Value;
                    var permissionName = perm.Key;
                    var sourceFiles = string.Empty;
                    var applicationPermission = (item.ApplicationPermission != null).ToString();
                    var delegatePermission = (item.DelegatePermission != null).ToString();

                    if (item.DocPermissions != null && item.DocPermissions.Count > 0)
                    {    
                        foreach (var docPerm in item.DocPermissions)
                        {
                            sourceFiles += docPerm.SourceFile + "|";
                        }
                    }
                    var length = sourceFiles.Length > 100 ? 100 : sourceFiles.Length;
                    sourceFiles = sourceFiles.Substring(0, length);
                    wrt.WriteField(permissionName);
                    wrt.WriteField(applicationPermission);
                    wrt.WriteField(delegatePermission);
                    wrt.WriteField(sourceFiles);
                    wrt.NextRecord();
                }

            }
        }
    }
}
