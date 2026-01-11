using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NTwain.Sidecar.Twain;

namespace NTwain.Sidecar;

internal class Program
{
    public static IServiceProvider Services { get; private set; } = null!;

    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register configuration

        // Add CORS for cross-origin requests
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        var app = builder.Build();
        Services = app.Services;

        app.UseDeveloperExceptionPage();

        // Enable CORS
        app.UseCors();


        app.Map("/test", () => SourceEnumerator.GetAllSourcesAsync());

        app.Run();
    }
}
