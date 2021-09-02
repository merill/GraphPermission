using GraphMarkdown.Data;
using GraphMarkdown.Infrastructure;
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

        public MarkdownGenerator(Config config)
        {
            _config = config;
        }

        public async Task<Dictionary<string, GraphPermissionMap>> GenerateAsync(List<DocGraphPermission> docPermissions, string rootFolderPath)
        {
            var graphResponse = await GetGraphResponseObject();
            var permissionMap = MapPermissions(docPermissions, graphResponse);

            var permFolderPath = Path.Combine(rootFolderPath, "graphpermission");

            foreach (var perm in permissionMap)
            {
                CreateMarkdown(perm.Value, permFolderPath);
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

            foreach (var perm in perms.OrderBy(i => i.Value.PermissionName))
            {
                var permName = perm.Value.PermissionName;
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

                sbIndex.AppendLine($"* [{perm.Value.PermissionName}]({url}.md)");
            }

            CreateFile(permFolderPath, "toc.yml", sbYml.ToString());
            CreateFile(permFolderPath, "index.md", sbIndex.ToString());
            CreateFile(rootFolderPath, "index.md", sbIndex.ToString().Replace("(", "(graphpermission/"));

            var rootToc = @"
- name: Graph Permissions
  href: graphpermission/
  homepage: graphpermission/index.md
- name: About
  href: about/
  homepage: about/index.md
";

            CreateFile(rootFolderPath, "toc.yml", rootToc);
        }

        private void CreateFile(string folderPath, string fileName, string content)
        {
            new DirectoryInfo(folderPath).Create();
            var filePath = Path.Combine(folderPath, fileName);
            File.WriteAllText(filePath, content);
        }
        private void CreateMarkdown(GraphPermissionMap perm, string folderPath)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"# {perm.PermissionName}"); sb.AppendLine();

            sb.AppendLine($"## Application Permission"); sb.AppendLine();
            if (perm.ApplicationPermission == null)
            {
                sb.AppendLine("N/A"); sb.AppendLine();
            }
            else
            {
                sb.AppendLine($"{perm.ApplicationPermission.id}"); sb.AppendLine();
                sb.AppendLine($"{perm.ApplicationPermission.displayName}"); sb.AppendLine();
                sb.AppendLine($"{perm.ApplicationPermission.description}"); sb.AppendLine();
            }

            sb.AppendLine($"## Delegate Permission"); sb.AppendLine();

            if (perm.DelegatePermission == null)
            {
                sb.AppendLine("N/A"); sb.AppendLine();
            }
            else
            {
                sb.AppendLine($"{perm.DelegatePermission.id}"); sb.AppendLine();
                sb.AppendLine($"Consent Type: {perm.DelegatePermission.type}"); sb.AppendLine();
                sb.AppendLine("Admin Description"); sb.AppendLine();
                sb.AppendLine($"{perm.DelegatePermission.adminConsentDisplayName}"); sb.AppendLine();
                sb.AppendLine($"{perm.DelegatePermission.adminConsentDescription}"); sb.AppendLine();

                sb.AppendLine("User Description"); sb.AppendLine();
                sb.AppendLine($"{perm.DelegatePermission.userConsentDisplayName}"); sb.AppendLine();
                sb.AppendLine($"{perm.DelegatePermission.userConsentDescription}"); sb.AppendLine();
            }

            sb.AppendLine($"## Graph Methods"); sb.AppendLine();
            sb.AppendLine($"### V1"); sb.AppendLine();
            var v1Uris = from p in perm.Uris where p.Value.IsV1 && !p.Value.IsBeta select p;
            AppendUri(sb, v1Uris, false);
            sb.AppendLine($"### Beta"); sb.AppendLine();
            var betaUris = from p in perm.Uris where p.Value.IsBeta && !p.Value.IsV1 select p;
            AppendUri(sb, betaUris, true);
            sb.AppendLine($"### V1 + Beta"); sb.AppendLine();
            var v1BetaUris = from p in perm.Uris where p.Value.IsBeta && p.Value.IsV1 select p;
            AppendUri(sb, v1BetaUris, false);


            CreateFile(folderPath, $"{perm.PermissionName}.md", sb.ToString());
        }

        private void AppendUri(StringBuilder sb, IEnumerable<KeyValuePair<string, ApiUri>> uris, bool isBeta)
        {
            var apiVersion = isBeta ? "graph-rest-beta" : "graph-rest-1.0";
            
            foreach (var uri in uris)
            {
                var docName = isBeta ? uri.Value.SourceDocBeta : uri.Value.SourceDocV1;
                var docUri = $"https://docs.microsoft.com/en-us/graph/api/{docName}?view={apiVersion}&tabs=http";
                sb.AppendLine($"* [{uri.Value.Uri}]({docUri})");
                sb.AppendLine();
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
                    Console.WriteLine("DocPerm not in Graph: {0}", permMap.DocPermissions[0].PermissionName);
                }

                if (permMap.DocPermissions.Count == 0)
                {
                    if (permMap.ApplicationPermission == null)
                    {
                        Console.WriteLine("AppPerm not in Doc: {0}", permMap.PermissionName);
                    }
                    if (permMap.DelegatePermission == null)
                    {
                        Console.WriteLine("DelPerm not in Doc: {0}", permMap.PermissionName);
                    }
                }
            }

            return permissionMap;
        }

        private static void AddApiUri(Dictionary<string, GraphPermissionMap> permissionMap)
        {
            foreach(var permItem in permissionMap)
            {
                var perm = permItem.Value;
                perm.Uris = new Dictionary<string, ApiUri>();
                foreach(var docPerm in perm.DocPermissions)
                {
                    ApiUri apiUri;
                    if (!perm.Uris.TryGetValue(docPerm.HttpRequest, out apiUri))
                    {
                        apiUri = new ApiUri() { Uri = docPerm.HttpRequest };
                        perm.Uris.Add(docPerm.HttpRequest, apiUri);
                    }
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
            if(permMap.ApplicationPermission == null)
            {
                var isAppPermission = (from p in permMap.DocPermissions 
                                       where p.PermissionType.Contains("application", StringComparison.InvariantCultureIgnoreCase) 
                                       select p).FirstOrDefault() != null;
                if(isAppPermission)
                {
                    permMap.ApplicationPermission = new Approle() { value = permMap.PermissionName };
                }
            }

            if (permMap.DelegatePermission == null)
            {
                var isDelegatePermission = (from p in permMap.DocPermissions
                                       where p.PermissionType.Contains("delegate", StringComparison.InvariantCultureIgnoreCase)
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
