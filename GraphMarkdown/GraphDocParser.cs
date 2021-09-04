using GraphMarkdown.Data;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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

            List<DocGraphPermission> permissions = ParsePermissions(filePath, isBeta, doc);

            var resources = (from p in doc.Descendants<LinkInline>()
                            where p.Url != null && p.Url.StartsWith("../resources", StringComparison.InvariantCultureIgnoreCase)
                            select Path.GetFileNameWithoutExtension(p.Url)).Distinct().ToList();
            if(resources.Count > 0)
            {
                foreach(var perm in permissions)
                {
                    perm.Resources = resources;
                }
            }
            return permissions;
        }

        private static List<DocGraphPermission> ParsePermissions(string filePath, bool isBeta, MarkdownDocument doc)
        {
            var permGraphTable = 
                from p in doc.Descendants<Table>()
                where p.Descendants<LiteralInline>().FirstOrDefault() != null
                    && p.Descendants<LiteralInline>().FirstOrDefault().Content.ToString().Equals("Permission type", StringComparison.CurrentCultureIgnoreCase)
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
                                        .Replace("For user:", ",")
                                        .Replace("For device:", ",")
                                        .Replace("(see below).", ",")
                                        .Replace("Profile photo of the signed-in user:", ",")
                                        .Replace("In teams:", ",")
                                        .Replace("In channels:", ",")
                                        .Replace("chat resource:", ",")
                                        .Replace("For channel resource:", ",")
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

        public Dictionary<string, DocResource> GetResourcesInFolder(string apiReferenceFolder)
        {
            Dictionary<string, DocResource> resources = new();

            // Default to v1 resource and only use beta if a corresponding v1 is not available.
            // v1
            var v1FolderPath = Path.Combine(apiReferenceFolder, @"v1.0\resources");
            foreach (var filePath in Directory.GetFiles(v1FolderPath))
            {
                var resource = GetResourceInFile(filePath);
                if (resource != null) { resources.Add(Path.GetFileNameWithoutExtension(filePath), resource); }
            }

            // beta
            var betaFolderPath = Path.Combine(apiReferenceFolder, @"beta\resources");
            foreach (var filePath in Directory.GetFiles(betaFolderPath))
            {
                var resourceName = Path.GetFileNameWithoutExtension(filePath);
                if (!resources.ContainsKey(resourceName))
                {
                    var resource = GetResourceInFile(filePath);
                    if (resource != null) { resources.Add(resourceName, resource); }
                }
            }
            return resources;
        }

        private DocResource GetResourceInFile(string filePath)
        {
            var md = File.ReadAllText(filePath);

            //TODO: If file is subscription-get need to parse table differently (deleage and app perms are in columns). 
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

            var doc = Markdown.Parse(md, pipeline);

            var docResource = new DocResource()
            {
                SourceFile = Path.GetFileNameWithoutExtension(filePath)
            };

            var x = from p in doc.Descendants<HeadingBlock>() select p;
            var resourceName = (from p in doc.Descendants<HeadingBlock>()
                                where p.Inline.FirstChild.ToString().EndsWith("resource type", StringComparison.InvariantCultureIgnoreCase)
                                select p.Inline.FirstChild.ToString()).FirstOrDefault();
            if(resourceName != null)
            {
                docResource.ResourceName = resourceName.ToString();
            }

            var propertyTable = 
                (from p in doc.Descendants<Table>()
                where p.Descendants<LiteralInline>().FirstOrDefault() != null && 
                    p.Descendants<LiteralInline>().FirstOrDefault().Content.ToString().Equals("Property", StringComparison.CurrentCultureIgnoreCase)
                 select p).FirstOrDefault();

            if(propertyTable != null)
            {
                docResource.PropertiesMarkdown = md.Substring(propertyTable.Span.Start - 1, propertyTable.Span.Length + 2);
                //Remove hyperlinks from the properties
                docResource.PropertiesMarkdown = Regex.Replace(docResource.PropertiesMarkdown, @"\[(.*?)\]\(.*?\)", "$1");
            }
            return docResource;
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