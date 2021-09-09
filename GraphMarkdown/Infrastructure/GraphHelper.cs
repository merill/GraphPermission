using GraphMarkdown.Data;
using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GraphMarkdown.Infrastructure
{
    internal class GraphHelper
    {
        Config _config;
        public GraphHelper(Config config)
        {
            _config = config;
        }
        public async Task<string> GetGraphResponseAsync(string uri)
        {


            var app = ConfidentialClientApplicationBuilder.Create(_config.ClientId  )
                                                      .WithClientSecret(_config.ClientSecret)
                                                      .WithAuthority(new Uri(_config.Authority))
                                                      .Build();
            var result = await app.AcquireTokenForClient(new string[]{ "https://graph.microsoft.com/.default"})
                  .ExecuteAsync();

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            HttpResponseMessage response = await httpClient.GetAsync(uri);
            return await response.Content.ReadAsStringAsync();

        }

        public static string GetMicrosoftGraphDocLinkMarkdown(string title, string docNameV1, string docNameBeta, bool isResource, bool isV1)
        {
            var docUri = GetMicrosoftGraphDocLink(docNameV1, docNameBeta, isResource, isV1);
            
            return $"[{title}]({docUri})";
        }

        public static string GetMicrosoftGraphDocLink(string docNameV1, string docNameBeta, bool isResource, bool isV1)
        {
            var apiVersion = "graph-rest-1.0";
            var docName = docNameV1;
            if (!isV1)
            {
                apiVersion = "graph-rest-beta";
                docName = docNameBeta;
            }
            var resoucePath = isResource ? "resources/" : "";
            return $"https://docs.microsoft.com/graph/api/{resoucePath}{docName}?view={apiVersion}&tabs=http";
        }


        public static string GetGraphPermUri(string permissionName)
        {
            return $"https://graphpermissions.merill.net/permission/{permissionName}.html";
        }
    }
}
