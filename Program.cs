using System.Runtime.CompilerServices;
using OpenAI_API.Moderation;

namespace cloudinteractive.documentcloud
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();


            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapGet("/status", (HttpContext httpContext) =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                        new WorkerPool
                        {
                            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                            TemperatureC = Random.Shared.Next(-20, 55),
                        })
                    .ToArray();
                return forecast;
            });

            app.MapPost("/request", (HttpContext httpContext, IFormFile file) =>
            {
                if (file is null || file.Length == 0)
                    return Results.BadRequest("File cannot be empty.");

                return Results.Ok();
            });


            app.Run();
        }
    }
}