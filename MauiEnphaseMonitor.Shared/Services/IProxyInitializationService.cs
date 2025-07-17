namespace MauiEnphaseMonitor.Shared.Services;

/// <summary>
/// Optional service for web projects to initialize CORS proxy.
/// MAUI projects can ignore this service.
/// </summary>
public interface IProxyInitializationService
{
    Task<bool> InitializeProxyAsync();
    Task<bool> IsProxyAvailableAsync();
    Task<string> GetProxyStatusAsync();
}