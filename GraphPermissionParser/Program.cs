﻿using GraphMarkdown;
using GraphMarkdown.Data;
using GraphMarkdown.Infrastructure;
using System;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using Microsoft.Extensions.Configuration;

namespace GraphPermissionParser
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var apiFolder = args[0];
            var docfxFolder = args[1];
            var siteFolder = Path.Combine(docfxFolder, "_site");
            var logger = InitializeLogger(docfxFolder);

            try
            {
                var parser = new GraphDocParser(logger);

                var permissions = parser.GetPermissionsInFolder(apiFolder);

                //var filePath = @"F:\code\microsoft-graph-docs\api-reference\beta\api\driveitem-get.md";
                //var permissions = parser.GetPermissionsInFile(filePath, false);

                var resources = parser.GetResourcesInFolder(apiFolder);

                var config = GetConfig();
                var mdg = new MarkdownGenerator(config, logger);

                var permissionMap = await mdg.GenerateAsync(permissions, resources, docfxFolder);

                var csvFilePath = Path.Combine(siteFolder, "permission.csv");
                Csv.SavePermissionsToCsv(permissionMap, csvFilePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occured");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }

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

        private static ILogger InitializeLogger(string docfxFolder)
        {
            var logTemplate = "{Message}{NewLine}{NewLine}";

            var logFile = Path.Combine(docfxFolder, "log.md");
            var log = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .WriteTo.File(logFile, Serilog.Events.LogEventLevel.Verbose, logTemplate, shared: true)
                .CreateLogger();

            return log;
        }
    }
}
