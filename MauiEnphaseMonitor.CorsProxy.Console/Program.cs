using MauiEnphaseMonitor.CorsProxy.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure to use port 8080
builder.WebHost.UseUrls("http://localhost:8080");

// Add services to the container
builder.Services.AddEnvoyProxyServices();
builder.Services.AddCorsProxyApi();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors("AllowAll");

// Map API endpoints
app.MapEnvoyProxyEndpoints();

// Health check endpoint
app.MapGet("/", () => "Enphase CORS Proxy Console - Ready");

// Start the application
Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                    Enphase CORS Proxy Console                 ║");
Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
Console.WriteLine("║ Status: Starting on http://localhost:8080                     ║");
Console.WriteLine("║                                                                ║");
Console.WriteLine("║ Available endpoints:                                           ║");
Console.WriteLine("║   POST /api/token          - Initialize proxy with JWT token  ║");
Console.WriteLine("║   GET  /api/token/status   - Check token status               ║");
Console.WriteLine("║   GET  /api/envoy/panels   - Get panel data                   ║");
Console.WriteLine("║   GET  /api/envoy/livedata - Get live energy data             ║");
Console.WriteLine("║   POST /api/envoy/stream   - Enable streaming                 ║");
Console.WriteLine("║   GET  /api/envoy/test     - Test connection                  ║");
Console.WriteLine("║                                                                ║");
Console.WriteLine("║ Press Ctrl+C to stop                                          ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");

app.Run();
