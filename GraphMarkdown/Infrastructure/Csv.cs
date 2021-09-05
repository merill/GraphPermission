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
            new DirectoryInfo(Path.GetDirectoryName(filePath)).Create();

            using StreamWriter outputFile = new StreamWriter(filePath);
            using var wrt = new CsvWriter(outputFile, CultureInfo.CurrentCulture);
            wrt.WriteField("PermissionName");
            wrt.WriteField("ApplicationPermission");
            wrt.WriteField("DelegatePermission");
            wrt.WriteField("Description");
            wrt.WriteField("ApiCount");
            wrt.WriteField("ApplicationGuid");
            wrt.WriteField("DelegateGuid");            
            wrt.WriteField("SourceFiles");
            wrt.WriteField("View");
            wrt.NextRecord();

            foreach (var perm in permMap)
            {
                var item = perm.Value;
                var permissionName = perm.Key;
                var sourceFiles = string.Empty;
                var applicationPermission = false.ToString();
                var delegatePermission = false.ToString();
                var applicationPermissionGuid = string.Empty;
                var delegatePermissionGuid = string.Empty;
                if (item.ApplicationPermission != null)
                {
                    applicationPermission = true.ToString();
                    applicationPermissionGuid = item.ApplicationPermission.id;
                }
                if (item.DelegatePermission != null)
                {
                    delegatePermission = true.ToString();
                    delegatePermissionGuid = item.DelegatePermission.id;
                }

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
                wrt.WriteField(item.DisplayName);
                wrt.WriteField(item.UriCount.ToString());
                wrt.WriteField(applicationPermissionGuid);
                wrt.WriteField(delegatePermissionGuid);
                wrt.WriteField(sourceFiles);
                wrt.WriteField($"https://graphpermissions.merill.net/permission/{permissionName}.html");
                wrt.NextRecord();
            }
        }
    }
}
