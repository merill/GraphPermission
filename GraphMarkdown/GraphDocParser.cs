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

        public List<DocGraphPermission> GetPermissionsInFile(string filePath, bool isBeta)
        {
            var md = File.ReadAllText(filePath);

            //TODO: If file is subscription-get need to parse table differently (deleage and app perms are in columns). 
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

                        var permission = string.Empty;
                        foreach (var cell in cells[1].Descendants<LiteralInline>())
                        {
                            permission += cell.Content.ToString();
                        }
                        permission = permission
                                        .Replace(" and ", ",")
                                        .Replace(" or ", ",")
                                        .Replace(" plus ", ",")
                                        .Replace("either ", ",")
                                        .Replace("For user resource:", ",")
                                        .Replace("For group resource:", ",")
                                        .Replace("For contact resource:", ",")
                                        .Replace("Role required to create subscription,Subscription.Read.All (see below).", "Subscription.Read.All")
                                        .Replace("Role required to create subscription.", "")
                                        .Replace("Permission required to create subscription.", "")
                                        .Replace("for a chat message.", ",")
                                        .Replace("for a channel message.", ",")
                                        .Replace("One from ", ",")
                                        .Replace("plus either", ",")
                                        .Replace("For user:",",")
                                        .Replace("For device:", ",")
                                        .Replace("(see below).", ",")
                                        .Replace("Profile photo of the signed-in user:", ",")
                                        .Replace(Environment.NewLine, ",")
                                        ;
                        foreach (var perm in permission.Split(','))
                        {
                            if (!string.IsNullOrWhiteSpace(perm))
                            {
                                if (IsValidPermission(perm))
                                {
                                    var permissionName = perm.Replace("*", "#").Trim(); //* is an invalid file name
                                    foreach (var httpRequest in httpRequests)
                                    {
                                        if (!string.IsNullOrWhiteSpace(httpRequest))
                                        {
                                            permissions.Add(new DocGraphPermission()
                                            {
                                                PermissionType = permissionType,
                                                PermissionName = permissionName,
                                                HttpRequest = httpRequest,
                                                SourceFile = Path.GetFileNameWithoutExtension(filePath),
                                                IsBeta = isBeta
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

        public List<DocGraphPermission> GetPermissionsInFolder(string apiReferenceFolder)
        {
            List<DocGraphPermission> permissions = new();

            //v1
            var v1FolderPath = Path.Combine(apiReferenceFolder, @"v1.0\api");
            foreach (var filePath in Directory.GetFiles(v1FolderPath))
            {
                permissions.AddRange(GetPermissionsInFile(filePath, false));
            }

            //beta
            var betaFolderPath = Path.Combine(apiReferenceFolder, @"beta\api");
            foreach (var filePath in Directory.GetFiles(betaFolderPath))
            {
                permissions.AddRange(GetPermissionsInFile(filePath, true));
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
            if (perm.StartsWith("#") || perm.StartsWith("*")) { return false; }
            return true;
        }
    }
}