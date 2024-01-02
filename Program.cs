using cloudinteractive.document.Util;
using Microsoft.AspNetCore.Mvc;

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

            app.UseAuthorization();

            try
            {
                //ApplicationLogging Init.
                ApplicationLogging.LoggerFactory = app.Services.GetService<ILoggerFactory>();
                _logger = ApplicationLogging.CreateLogger<Program>();

                //Get third-party API credentials from server.
                _setEnvironment();

                //Redis Connection
                var config = app.Services.GetService<IConfiguration>();
                RequestManagement.Init(config);

                //Run Worker threads.
                WorkerPool.StartThread(8);
            }
            catch (Exception e)
            {
                _logger.LogCritical("Init failed!\n" + e.ToString());
                return;
            }

            //Minimal API Mapping.
            app.MapGet("v1/document/status", (string requestId, ILogger <Program> logger, HttpRequest request) =>
            {
                if (System.String.IsNullOrWhiteSpace(requestId)) return Results.BadRequest(new { Id = 0, Error = "requestId cannot be empty."});
                var status = RequestManagement.GetRequestStatus(requestId);

                if (status is null) return Results.BadRequest(new { Id = 1, Error = "Invalid requestId" });

                return Results.Ok(new {status = (int)status});
            });

            app.MapGet("v1/document/result", (string requestId, ILogger<Program> logger, HttpRequest request) =>
            {
                if (System.String.IsNullOrWhiteSpace(requestId)) return Results.BadRequest(new { Id = 0, Error = "requestId cannot be empty." });
                var result = @RequestManagement.GetRequestResult(requestId);


                if (result is null) return Results.BadRequest(new { Id = 2, Error = "There is no results for provided requestId." });
                return Results.Text(@result, "application/json");

            });

            app.MapPost("v1/document/request", async (ILogger<Program> logger, IFormFile file, HttpRequest request) => {
                if (file is null || file.Length == 0)
                    return Results.BadRequest(new {Id = 3, Error = "File cannot be empty."});

                string? fileType = request.Form["fileType"];
                string? prompt = request.Form["prompt"];

                if (fileType is null)
                {
                    return Results.BadRequest(new { Id = 4, Error = "FileType cannot be empty."});
                }

                if (prompt is null)
                {
                    return Results.BadRequest(new { Id = 5, Error = "Prompt cannot be empty."});
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