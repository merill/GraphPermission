using GraphMarkdown.Data;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GraphMarkdown
{
    public class GraphDocParser
    {

        public List<DocGraphPermission> GetPermissionsInFile(string filePath)
        {
            var md = File.ReadAllText(filePath);
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

            var doc = Markdown.Parse(md, pipeline);

            var permGraphTable = from p in doc.Descendants<Table>()
                                 where p.Descendants<LiteralInline>().FirstOrDefault() != null
                                    && p.Descendants<LiteralInline>().FirstOrDefault().Content.ToString() == "Permission type"
                                 select p;

            var permissions = new List<DocGraphPermission>();
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
                        foreach (var perm in permission.Split(','))
                        {
                            if (!string.IsNullOrWhiteSpace(perm))
                            {
                                if (IsValidPermission(perm))
                                {
                                    foreach (var httpRequest in httpRequests)
                                    {
                                        if (!string.IsNullOrWhiteSpace(httpRequest))
                                        {
                                            permissions.Add(new DocGraphPermission()
                                            {
                                                PermissionType = permissionType,
                                                PermissionName = perm.Trim(),
                                                HttpRequest = httpRequest,
                                                SourceFile = Path.GetFileNameWithoutExtension(filePath)
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (permissions.Count == 0)
            {
                Console.WriteLine("Warning: No permissions found on {0}", filePath);
            }
            return permissions;
        }

        public List<DocGraphPermission> GetPermissionsInFolder(string folder)
        {
            List<DocGraphPermission> permissions = new();

            foreach (var filePath in Directory.GetFiles(folder))
            {
                permissions.AddRange(GetPermissionsInFile(filePath));
            }

            return permissions;
        }

        private static bool IsValidPermission(string perm)
        {
            var ignorePerms = new string[] { "not supported", "not applicable", "none", "n/a" };
            foreach (var ignore in ignorePerms)
            {
                if (perm.Contains(ignore, StringComparison.InvariantCultureIgnoreCase)) { return false; }
            }
            return true;
        }
    }
}