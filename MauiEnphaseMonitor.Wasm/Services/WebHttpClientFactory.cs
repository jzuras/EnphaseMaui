using MauiEnphaseMonitor.Shared.Services;

namespace MauiEnphaseMonitor.Wasm;

public class WebHttpClientFactory : IEnvoyHttpClientFactory
{
    public HttpClient CreateEnvoyClient()
    {
        // Web browsers use CORS proxy to access envoy.local
        var client = new HttpClient();
        client.BaseAddress = new Uri("http://localhost:8080/api/envoy/");
        client.Timeout = TimeSpan.FromSeconds(30);
        
        System.Diagnostics.Debug.WriteLine($"Web.Client: Created HttpClient for CORS proxy at localhost:8080");
        return client;
    }
}