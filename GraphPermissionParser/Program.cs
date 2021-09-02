using GraphMarkdown;
using GraphMarkdown.Data;
using GraphMarkdown.Infrastructure;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace GraphPermissionParser
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var parser = new GraphDocParser();
            var apiFolder = args[0];
            var permissions = parser.GetPermissionsInFolder(apiFolder);

            //var filePath = @"F:\code\microsoft-graph-docs\api-reference\v1.0\api\workforceintegration-post.md";
            //var permissions = parser.GetPermissionsInFile(filePath, false);

            var config = GetConfig();
            var mdg = new MarkdownGenerator(config);
            var mdFolder = args[1];
            var permissionMap = await mdg.GenerateAsync(permissions, mdFolder);
#if DEBUG
            var csvFilePath = @"F:\Code\GraphPermission\docfx_project\graphperm.csv";
            Csv.SavePermissionsToCsv(permissionMap, csvFilePath);
#endif

        }

        private static Config GetConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>()
                .Build();

            return new Config()
            {
                ClientId = config.GetSection("ClientId").Value,
                ClientSecret = config.GetSection("ClientSecret").Value,
                Authority = config.GetSection("Authority").Value
            };
        }
    }
}
