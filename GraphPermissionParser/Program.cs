using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System;
using System.Collections.Generic;
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
            foreach(var filePath in Directory.GetFiles(folder))
            {
                permissions.AddRange(GetPermissions(filePath));
            }
            //doc.IndexOf(new Markdig.Syntax.HeadingBlock() { Inline = new Markdig.Syntax.Inlines.ContainerInline() { } })
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
            var httpRequest = codeBlock.Lines.ToString();

            foreach (var table in permGraphTable)
            {
                foreach (var row in table.Descendants<TableRow>())
                {
                    if (!row.IsHeader)
                    {
                        var cells = row.Descendants<TableCell>().ToList();
                        var permissionType = cells[0].Descendants<LiteralInline>().FirstOrDefault().Content.ToString();
                        var permission = cells[1].Descendants<LiteralInline>().FirstOrDefault().Content.ToString();
                        permissions.Add(new GraphPermission()
                        {
                            PermissionType = permissionType,
                            Permission = permission,
                            HttpRequest = httpRequest,
                            SourceFile = filePath
                        });
                    }
                }
            }

            return permissions;
        }
    }
}
