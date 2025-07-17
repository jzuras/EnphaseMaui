namespace MauiEnphaseMonitor.Shared.Services;

public interface IEnvoyHttpClientFactory
{
    HttpClient CreateEnvoyClient();
}