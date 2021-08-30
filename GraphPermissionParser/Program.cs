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
            var apiFolder = @"F:\code\microsoft-graph-docs\api-reference\v1.0\api\";
            apiFolder = args[0];
            var permissions = parser.GetPermissionsInFolder(apiFolder);

            //var filePath = @"F:\code\microsoft-graph-docs\api-reference\v1.0\api\intune-rbac-deviceandappmanagementroledefinition-delete.md";
            //var permissions = parser.GetPermissionsInFile(filePath);

            var config = GetConfig();
            var mdg = new MarkdownGenerator(config);
            var mdFolder = @"F:\Code\temp\graphpermdoc\docfx_project";
            mdFolder = args[1];
            var result = await mdg.GenerateAsync(permissions, mdFolder);
            //var csvFilePath = @"F:\code\temp\graphperm.csv";
            //Csv.SavePermissionsToCsv(permissions, csvFilePath);

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
