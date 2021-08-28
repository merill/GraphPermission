using GraphMarkdown.Data;
using GraphMarkdown.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

        public async Task<bool> GenerateAsync(List<DocGraphPermission> docPermissions, string folderPath)
        {
            var graphResponse = await GetGraphResponseObject();
            var permissionMap = MapPermissions(docPermissions, graphResponse);

            foreach(var perm in permissionMap)
            {
                CreateMarkdown(perm.Value, folderPath);
            }
            return true;
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

            foreach(var docPerm in perm.DocPermissions)
            {
                sb.AppendLine($"* [{docPerm.HttpRequest}](https://docs.microsoft.com/en-us/graph/api/{docPerm.SourceFile}?view=graph-rest-1.0&tabs=http)");
                sb.AppendLine();
            }
            var filePath = Path.Combine(folderPath, $"{perm.PermissionName}.md");
            File.WriteAllText(filePath, sb.ToString());
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

        private async Task<MicrosoftGraphObject> GetGraphResponseObject()
        {
            var graphRequest = "https://graph.microsoft.com/v1.0/serviceprincipals?$filter=appId eq '00000003-0000-0000-c000-000000000000'&$top=1";

            var gh = new GraphHelper(_config);

            var responseJson = await gh.GetGraphResponseAsync(graphRequest);
            return JsonSerializer.Deserialize<GraphResponse>(responseJson).value[0];
        }
    }
}
