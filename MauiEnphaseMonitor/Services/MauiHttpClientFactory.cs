using MauiEnphaseMonitor.Shared.Services;

namespace MauiEnphaseMonitor.Services;

public class MauiHttpClientFactory : IEnvoyHttpClientFactory
{
    public HttpClient CreateEnvoyClient()
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => 
        {
            // Allow all SSL certificates for envoy.local
            System.Diagnostics.Debug.WriteLine($"SSL Certificate validation for: {message?.RequestUri?.Host}");
            System.Diagnostics.Debug.WriteLine($"SSL Errors: {errors}");
            return true;
        };

        var client = new HttpClient(handler);
        client.BaseAddress = new Uri("https://envoy.local/");
        client.Timeout = TimeSpan.FromSeconds(30);
        
        System.Diagnostics.Debug.WriteLine($"MAUI: Created new HttpClient with SSL bypass");
        return client;
    }
}