using GraphMarkdown.Data;
using GraphMarkdown.Infrastructure;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace GraphMarkdown
{
    public class MarkdownGenerator
    {
        Config _config;
        static ILogger _logger;

        private const string PermissionFolderName = "permission";

        public MarkdownGenerator(Config config, ILogger logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<Dictionary<string, GraphPermissionMap>> GenerateAsync(
            List<DocGraphPermission> docPermissions, Dictionary<string, DocResource> docResources, string rootFolderPath)
        {
            var graphResponse = await GetGraphResponseObject();
            var permissionMap = MapPermissions(docPermissions, graphResponse);

            var permFolderPath = Path.Combine(rootFolderPath, PermissionFolderName);

            foreach (var perm in permissionMap)
            {
                CreateMarkdown(perm.Value, docResources, permFolderPath);
            }

            CreateTocMarkdown(permissionMap, rootFolderPath, permFolderPath);

            return permissionMap;
        }

        private void CreateTocMarkdown(Dictionary<string, GraphPermissionMap> perms, string rootFolderPath, string permFolderPath)
        {
            var sbYml = new StringBuilder();
            var sbIndex = new StringBuilder();

            sbIndex.AppendLine("# Microsoft Graph Permission API"); sbIndex.AppendLine();
            sbIndex.AppendLine("Click through to a Graph Permission to view the APIs that can be called when the permission is consented to an application in your tenant."); sbIndex.AppendLine();

            sbIndex.AppendLine("# Permission Scopes"); sbIndex.AppendLine();

            sbYml.AppendLine($"- name: Overview");
            sbYml.AppendLine($"  href: index.md");
            var lastPermission = "";
            sbIndex.AppendLine("|Permission|Description|");
            sbIndex.AppendLine("|-|-|");
            foreach (var item in perms.OrderBy(i => i.Value.PermissionName))
            {
                var perm = item.Value;
                var permName = perm.PermissionName;
                var url = UrlEncoder.Default.Encode(permName);
                var permShortName = permName.Split(".")[0];
                if (lastPermission != permShortName)
                {
                    lastPermission = permShortName;
                    sbYml.AppendLine($"- name: '{permShortName}'");
                    sbYml.AppendLine($"  items:");
                }

                sbYml.AppendLine($"  - name: '{permName}'");
                sbYml.AppendLine($"    href: '{url}.md'");

                sbIndex.Append($"|[{perm.PermissionName}]({url}.md)");
                sbIndex.Append($"|{perm.DisplayName}");
                sbIndex.AppendLine();

                //sbIndex.AppendLine($"* [{perm.PermissionName}]({url}.md)");
            }

            CreateFile(permFolderPath, "toc.yml", sbYml.ToString());
            CreateFile(permFolderPath, "index.md", sbIndex.ToString());
            CreateFile(rootFolderPath, "index.md", sbIndex.ToString().Replace("(", $"({PermissionFolderName}/"));

        }

        private void CreateFile(string folderPath, string fileName, string content)
        {
            new DirectoryInfo(folderPath).Create();
            var filePath = Path.Combine(folderPath, fileName);
            File.WriteAllText(filePath, content);
        }
        private void CreateMarkdown(GraphPermissionMap perm, Dictionary<string, DocResource> docResources, string folderPath)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"# {perm.PermissionName}"); sb.AppendLine();

            if (perm.ApplicationPermission == null &&
                perm.DelegatePermission == null &&
                (perm.Uris == null || perm.Uris.Count == 0))
            {
                sb.AppendLine("> [!NOTE]"); sb.AppendLine();
                sb.AppendLine($"> The permission {perm.PermissionName} does not have any graph methods published."); sb.AppendLine();
            }
            else
            {
                if (!string.IsNullOrEmpty(perm.Description))
                {
                    sb.AppendLine($"> {perm.Description}");
                }

                AppendGraphMethods(perm, sb);

                AppendPermissionTypeInfo(perm, sb);

                AppendResources(perm, docResources, sb);
            }

            CreateFile(folderPath, $"{perm.PermissionName}.md", sb.ToString());
        }

        private void AppendResources(GraphPermissionMap perm, Dictionary<string, DocResource> docResources, StringBuilder sb)
        {
            var permResourceNames = (from p in perm.DocPermissions where p.Resources != null select p.Resources);
            var uniquePermResources = new Dictionary<string, DocResource>();


            foreach (var permResourceName in permResourceNames)
            {
                foreach (var resourceName in permResourceName)
                {
                    if (!uniquePermResources.ContainsKey(resourceName))
                    {
                        if (docResources.ContainsKey(resourceName))
                        {
                            uniquePermResources.Add(resourceName, docResources[resourceName]);
                        }
                    }
                }
            }
            if(uniquePermResources.Count > 0)
            {
                sb.AppendLine($"## Resources");
                foreach (var resource in uniquePermResources.OrderBy(i => i.Key))
                {
                    var res = resource.Value;
                    var name = string.IsNullOrEmpty(res.ResourceName) ? res.SourceFile : res.ResourceName;
                    var docUri = GraphHelper.GetMicrosoftGraphDocLink(name.Replace("resource type", ""), res.SourceFile, res.SourceFile, true, !res.IsBeta);
                    sb.AppendLine($"### {docUri}");
                    sb.AppendLine(res.PropertiesMarkdown);
                }
            }
        }

        private static void AppendPermissionTypeInfo(GraphPermissionMap perm, StringBuilder sb)
        {
            if (perm.DelegatePermission != null)
            {
                sb.AppendLine($"## Delegate Permission");
                sb.AppendLine($"|||");
                sb.AppendLine($"|-|-|");
                sb.AppendLine($"|**Id**|{perm.DelegatePermission.id}|");
                sb.AppendLine($"|**Consent Type**|{perm.DelegatePermission.type}|");
                sb.AppendLine($"|**Display String**|{perm.DelegatePermission.adminConsentDisplayName}|");
                sb.AppendLine($"|**Description**|{perm.DelegatePermission.adminConsentDescription}|");
            }

            if (perm.ApplicationPermission != null)
            {
                sb.AppendLine($"## Application Permission");
                sb.AppendLine($"|||");
                sb.AppendLine($"|-|-|");
                sb.AppendLine($"|**Id**|{perm.ApplicationPermission.id}|");
                sb.AppendLine($"|**Display String**|{perm.ApplicationPermission.displayName}|");
                sb.AppendLine($"|**Description**|{perm.ApplicationPermission.description}|");
            }
        }

        private static void AppendGraphMethods(GraphPermissionMap perm, StringBuilder sb)
        {
            sb.AppendLine($"## Graph Methods"); sb.AppendLine();
            if (perm.Uris == null || perm.Uris.Count == 0)
            {
                sb.AppendLine("> [!NOTE]");
                sb.AppendLine("> This permission does not have any graph methods published."); sb.AppendLine();
            }
            else
            {
                sb.AppendLine("Type: A = Application Permission, D = Delegate Permission"); sb.AppendLine();

                sb.AppendLine("|Ver|Type|Method|");
                sb.AppendLine("|-------|----|------|");
                foreach (var permission in perm.Uris.OrderBy(i => i.Value.Uri))
                {
                    var p = permission.Value;
                    var version = "V1";
                    if (!p.IsV1)
                    {
                        version = "Beta";
                    }
                    var permType = p.IsApplication && p.IsDelegate ? "A,D" :
                                    p.IsApplication ? "A" :
                                    p.IsDelegate ? "D" : "";

                    var docUri = GraphHelper.GetMicrosoftGraphDocLink(p.Uri, p.SourceDocV1, p.SourceDocBeta, false, p.IsV1);
                    sb.AppendLine($"|{version}|{permType}|{docUri}|");
                    
                }
            }
        }



        private static Dictionary<string, GraphPermissionMap> MapPermissions(List<DocGraphPermission> docPermissions, MicrosoftGraphObject graphResponse)
        {
            var permissionMap = new Dictionary<string, GraphPermissionMap>();


            foreach (var appPerm in graphResponse.appRoles)
            {
                if (!permissionMap.ContainsKey(appPerm.value))
                {
                    permissionMap.Add(appPerm.value, new GraphPermissionMap(appPerm.value) { });
                }
                permissionMap[appPerm.value].ApplicationPermission = appPerm;
            }

            foreach (var delegatePerm in graphResponse.oauth2PermissionScopes)
            {
                if (!permissionMap.ContainsKey(delegatePerm.value))
                {
                    permissionMap.Add(delegatePerm.value, new GraphPermissionMap(delegatePerm.value) { });
                }
                permissionMap[delegatePerm.value].DelegatePermission = delegatePerm;
            }

            foreach (var docPerm in docPermissions)
            {
                if (!permissionMap.ContainsKey(docPerm.PermissionName))
                {
                    permissionMap.Add(docPerm.PermissionName, new GraphPermissionMap(docPerm.PermissionName) { });
                }
                permissionMap[docPerm.PermissionName].DocPermissions.Add(docPerm);
            }

            foreach (var permMap in permissionMap.Values)
            {
                UpdateMissingGraphPermFromDoc(permMap);
                AddApiUri(permissionMap);
                
                if (permMap.ApplicationPermission == null && permMap.DelegatePermission == null)
                {
                    var permissionUri = GraphHelper.GetGraphPermUri(permMap.PermissionName);
                    _logger.Warning($"DocPerm not in Graph: [{permMap.PermissionName}]({permissionUri})");
                }

                if (permMap.DocPermissions.Count == 0)
                {
                    var permissionUri = GraphHelper.GetGraphPermUri(permMap.PermissionName);
                    if (permMap.ApplicationPermission == null)
                    {
                        _logger.Warning($"AppPerm not in Doc [{permMap.PermissionName}]({permissionUri})");
                    }
                    if (permMap.DelegatePermission == null)
                    {
                        _logger.Warning($"DelPerm not in Doc: [{permMap.PermissionName}]({permissionUri})");
                    }
                }
            }

            return permissionMap;
        }

        private static void AddApiUri(Dictionary<string, GraphPermissionMap> permissionMap)
        {
            foreach (var permItem in permissionMap)
            {
                var perm = permItem.Value;
                perm.Uris = new Dictionary<string, ApiUri>();
                foreach (var docPerm in perm.DocPermissions)
                {
                    ApiUri apiUri;
                    if (!perm.Uris.TryGetValue(docPerm.HttpRequest, out apiUri))
                    {
                        apiUri = new ApiUri() { Uri = docPerm.HttpRequest };
                        perm.Uris.Add(docPerm.HttpRequest, apiUri);
                    }
                    if (docPerm.IsApplicationPermission) { apiUri.IsApplication = true; }
                    if (docPerm.IsDelegatePermission) { apiUri.IsDelegate = true; }
                    if (docPerm.IsBeta)
                    {
                        apiUri.IsBeta = true;
                        apiUri.SourceDocBeta = docPerm.SourceFile;
                    }
                    if (!docPerm.IsBeta)
                    {
                        apiUri.IsV1 = true;
                        apiUri.SourceDocV1 = docPerm.SourceFile;
                    }
                }
            }
        }

        /// <summary>
        /// Sometimes perms are only in the doc and not in Graph, add them from 
        /// </summary>
        /// <param name="permMap"></param>
        private static void UpdateMissingGraphPermFromDoc(GraphPermissionMap permMap)
        {
            if (permMap.ApplicationPermission == null)
            {
                var isAppPermission = (from p in permMap.DocPermissions
                                       where p.IsApplicationPermission
                                       select p).FirstOrDefault() != null;
                if (isAppPermission)
                {
                    permMap.ApplicationPermission = new Approle() { value = permMap.PermissionName };
                }
            }

            if (permMap.DelegatePermission == null)
            {
                var isDelegatePermission = (from p in permMap.DocPermissions
                                            where p.IsDelegatePermission
                                            select p).FirstOrDefault() != null;
                if (isDelegatePermission)
                {
                    permMap.DelegatePermission = new Oauth2permissionscopes() { value = permMap.PermissionName };
                }
            }
        }

        private async Task<MicrosoftGraphObject> GetGraphResponseObject()
        {
            var graphRequest = "https://graph.microsoft.com/v1.0/serviceprincipals?$filter=appId eq '00000003-0000-0000-c000-000000000000'&$top=1";

            var gh = new GraphHelper(_config);

            var responseJson = await gh.GetGraphResponseAsync(graphRequest);
            return JsonSerializer.Deserialize<GraphResponse>(responseJson).value[0];
        }
    }
}
