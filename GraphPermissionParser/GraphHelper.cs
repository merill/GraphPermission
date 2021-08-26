using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GraphPermissionParser
{
    class GraphHelper
    {
        public static async Task<string> GetGraphResponseAsync(string uri)
        {
            var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<Program>()
            .Build();

            var app = ConfidentialClientApplicationBuilder.Create(config.GetSection("ClientId").Value)
                                                      .WithClientSecret(config.GetSection("ClientSecret").Value)
                                                      .WithAuthority(new Uri(config.GetSection("Authority").Value))
                                                      .Build();
            var result = await app.AcquireTokenForClient(new string[]{ "https://graph.microsoft.com/.default"})
                  .ExecuteAsync();

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            // Call the web API.
            HttpResponseMessage response = await httpClient.GetAsync(uri);
            return await response.Content.ReadAsStringAsync();

        }
    }
}
