using CsvHelper;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace GraphPermissionParser
{
    class Program
    {
        static void Main(string[] args)
        {
            List<GraphPermission> permissions = new List<GraphPermission>();
            var folder = @"F:\code\microsoft-graph-docs\api-reference\v1.0\api\";
            foreach (var filePath in Directory.GetFiles(folder))
            {
                permissions.AddRange(GetPermissions(filePath));
            }

            //permissions.AddRange(GetPermissions(@"F:\code\microsoft-graph-docs\api-reference\v1.0\api\intune-rbac-deviceandappmanagementroledefinition-delete.md"));

            using (var writer = new StreamWriter(@"F:\code\temp\graphperm.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(permissions);
            }

            Console.WriteLine("finidhe reading");
        }

        private static List<GraphPermission> GetPermissions(string filePath)
        {
            var md = File.ReadAllText(filePath);
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

            var doc = Markdown.Parse(md, pipeline);

            var permGraphTable = from p in doc.Descendants<Table>()
                                 where p.Descendants<LiteralInline>().FirstOrDefault() != null
                                    && p.Descendants<LiteralInline>().FirstOrDefault().Content.ToString() == "Permission type"
                                 select p;

            var permissions = new List<GraphPermission>();
            var codeBlock = doc.Descendants<FencedCodeBlock>().First();
            var httpRequests = codeBlock.Lines.ToString().Split(Environment.NewLine);

            foreach (var table in permGraphTable)
            {
                foreach (var row in table.Descendants<TableRow>())
                {
                    if (!row.IsHeader)
                    {
                        var cells = row.Descendants<TableCell>().ToList();
                        var permissionType = cells[0].Descendants<LiteralInline>().FirstOrDefault().Content.ToString();
                        var permission = cells[1].Descendants<LiteralInline>().FirstOrDefault().Content.ToString();
                        foreach(var perm in permission.Split(','))
                        {
                            if(!string.IsNullOrWhiteSpace(perm))
                            {
                                if (IsValidPermission(perm))
                                {
                                    foreach (var httpRequest in httpRequests)
                                    {
                                        if (!string.IsNullOrWhiteSpace(httpRequest))
                                        {
                                            permissions.Add(new GraphPermission()
                                            {
                                                PermissionType = permissionType,
                                                Permission = perm.Trim(),
                                                HttpRequest = httpRequest,
                                                SourceFile = filePath
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if(permissions.Count ==0)
            {
                Console.WriteLine("Warning: No permissions found on {0}", filePath);
            }
            return permissions;
        }

        private static bool IsValidPermission(string perm)
        {
            var ignorePerms = new string[] { "not supported", "not applicable", "none", "n/a" };
            foreach(var ignore in ignorePerms)
            {
                if (perm.Contains(ignore, StringComparison.InvariantCultureIgnoreCase)) { return false; }
            }
            return true;
        }
    }
}
