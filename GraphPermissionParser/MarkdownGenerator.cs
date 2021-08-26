using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphPermissionParser
{
    class MarkdownGenerator
    {
        public async Task<bool> GenerateAsync(List<GraphPermission> permissions, string folderPath)
        {
            var graphRequest = "https://graph.microsoft.com/v1.0/serviceprincipals?$filter=appId eq '00000003-0000-0000-c000-000000000000'";

            var graphO = await GraphHelper.GetGraphResponseAsync(graphRequest);

            return true;
        }
    }
}
