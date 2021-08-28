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
    }
}
