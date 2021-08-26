using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphPermissionParser
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var parser = new GraphDocParser();
            //var folder = @"F:\code\microsoft-graph-docs\api-reference\v1.0\api\";
            //var permissions = parser.GetPermissionsInFolder(folder);

            var filePath = @"F:\code\microsoft-graph-docs\api-reference\v1.0\api\intune-rbac-deviceandappmanagementroledefinition-delete.md";
            var permissions = parser.GetPermissionsInFile(filePath);

            var mdg = new MarkdownGenerator();
            var mdFolder = @"F:\temp\graphperms";
            var result = await mdg.GenerateAsync(permissions, mdFolder);
            var csvFilePath = @"F:\code\temp\graphperm.csv";
            CsvHelper.SavePermissionsToCsv(permissions, csvFilePath);

        }
    }
}
