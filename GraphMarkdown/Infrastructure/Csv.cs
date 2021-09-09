using CsvHelper;
using GraphMarkdown.Data;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace GraphMarkdown.Infrastructure
{
    public static class Csv
    {
        public static void SavePermissionsToCsv(Dictionary<string, GraphPermissionMap> permMap, List<DocGraphPermission> docGraphPermissions, string siteFolder)
        {
            var csvSummaryFilePath = Path.Combine(siteFolder, "permission.csv");
            CreatePermissionSummary(permMap, csvSummaryFilePath);

            var csvDetailFilePath = Path.Combine(siteFolder, "permissions.csv");
            CreatePermissionDetail(docGraphPermissions, csvDetailFilePath);
        }

        private static void CreatePermissionSummary(Dictionary<string, GraphPermissionMap> permMap, string filePath)
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

            foreach (var perm in permMap)
            {
                var item = perm.Value;
                var permissionName = perm.Key;
                var sourceFiles = string.Empty;
                var applicationPermission = false.ToString();
                var delegatePermission = false.ToString();
                var applicationPermissionGuid = string.Empty;
                var delegatePermissionGuid = string.Empty;

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

        private static void CreatePermissionDetail(List<DocGraphPermission> docPermissions, string filePath)
        {
            new DirectoryInfo(Path.GetDirectoryName(filePath)).Create();

            using StreamWriter outputFile = new StreamWriter(filePath);
            using var wrt = new CsvWriter(outputFile, CultureInfo.CurrentCulture);

            wrt.WriteField("PermissionName");
            wrt.WriteField("ApplicationPermission");
            wrt.WriteField("DelegatePermission");
            wrt.WriteField("IsBeta");
            wrt.WriteField("API");
            wrt.WriteField("DocUri");
            wrt.NextRecord();

            foreach (var perm in docPermissions)
            {
                wrt.WriteField(perm.PermissionName);
                wrt.WriteField(perm.IsApplicationPermission);
                wrt.WriteField(perm.IsDelegatePermission);
                wrt.WriteField(perm.IsBeta);
                wrt.WriteField(perm.HttpRequest);
                var docUri = GraphHelper.GetMicrosoftGraphDocLink(perm.SourceFile, perm.SourceFile, false, !perm.IsBeta);
                wrt.WriteField(docUri);
                wrt.NextRecord();
            }
        }
    }
}
