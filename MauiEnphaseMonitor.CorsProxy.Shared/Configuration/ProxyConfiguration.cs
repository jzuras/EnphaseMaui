namespace MauiEnphaseMonitor.CorsProxy.Shared.Configuration;

public class ProxyConfiguration
{
    public const string DefaultBaseUrl = "http://localhost:8080";
    public const string EnvoyBaseUrl = "https://envoy.local";
    
    public string BaseUrl { get; set; } = DefaultBaseUrl;
    public bool EnableCors { get; set; } = true;
    public bool EnableDetailedErrors { get; set; } = true;
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
}