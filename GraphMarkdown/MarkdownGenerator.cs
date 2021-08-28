using GraphMarkdown.Data;
using GraphMarkdown.Infrastructure;
using System.Collections.Generic;
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

        public async Task<bool> GenerateAsync(List<GraphPermission> permissions, string folderPath)
        {
            string responseJson = await GetGraphResponseObject();

            var graphResponse = JsonSerializer.Deserialize<GraphResponse>(responseJson);

            return true;
        }

        private async Task<string> GetGraphResponseObject()
        {
            var graphRequest = "https://graph.microsoft.com/v1.0/serviceprincipals?$filter=appId eq '00000003-0000-0000-c000-000000000000'&$top=1";

            var gh = new GraphHelper(_config);

            var responseJson = await gh.GetGraphResponseAsync(graphRequest);
            return responseJson;
        }
    }
}
