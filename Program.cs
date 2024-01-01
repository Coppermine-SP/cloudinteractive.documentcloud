using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using cloudinteractive.document.Util;
using NetTopologySuite.IO;

namespace cloudinteractive.documentcloud
{
    public class Program
    {
        private static ILogger? _logger;
        private static string _httpGet(string endpoint, string path)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(endpoint);
                return client.GetStringAsync(path).Result;
            }
        }

        private static void _setEnvironment()
        {
            //Below code is only works in CloudInteractive Corporate Network.
            const string endpoint = "https://secure.cloudint.corp";
            const string azure_endpoint = "endpoint/azure_cv";
            const string azure_key = "key/azure_cv";
            const string openai_key = "key/openai";

            _logger.LogInformation($"Get Credentials from {endpoint}...");
            AzureComputerVision.Init(_httpGet(endpoint, azure_endpoint),_httpGet(endpoint, azure_key));
            OpenAI.Init(_httpGet(endpoint, openai_key));
            _logger.LogInformation("Successfully got credentials from server.");
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("cloudinteractive.documentcloud\nCopyright (C) 2024 CloudInteractive Inc.\n\n");
            var builder = WebApplication.CreateBuilder(args);

            //Add services to the container.
            builder.Services.AddAuthorization();
            var app = builder.Build();

            //Configure the HTTP request pipeline.
            app.UseHttpsRedirection();
            app.UseAuthorization();

            //ApplicationLogging Init.
            ApplicationLogging.LoggerFactory = app.Services.GetService<ILoggerFactory>(); 
            _logger = ApplicationLogging.CreateLogger<Program>();

            //Get third-party API credentials from server.
            _setEnvironment();

            //Run Worker threads.
            WorkerPool.StartThread(8);
            
            //Minimal API Mapping.
            app.MapGet("/status", (ILogger <Program> logger, HttpContext httpContext) =>
            {
                return Results.Ok();
            });

            app.MapPost("v1/request/document", async (ILogger<Program> logger, IFormFile file, HttpRequest request) => {
                if (file is null || file.Length == 0)
                    return Results.BadRequest("File cannot be empty.");

                string? fileType = request.Form["fileType"];
                string? prompt = request.Form["prompt"];

                if (fileType is null)
                {
                    return Results.BadRequest("fileType cannot be null.");
                }

                if (prompt is null)
                {
                    return Results.BadRequest("prompt cannot be null.");
                }

                fileType = fileType.ToLower();
                FileType type;
                if (fileType == "pdf") type = FileType.PDF;
                else if (fileType == "image") type = FileType.Image;
                else return Results.BadRequest("invalid fileType.");

                try
                {
                    string id = await WorkerPool.CreateNewRequest(prompt, type, file.OpenReadStream());
                    return Results.Ok(new { requestId = id });
                }
                catch (Exception e)
                {
                    _logger.LogError("Failed to create new request: " + e.ToString());
                    return Results.BadRequest("Internal Server Error.");
                }
            });


            app.Run();
        }
    }
}