using MauiEnphaseMonitor.CorsProxy.Shared.Models;

namespace MauiEnphaseMonitor.CorsProxy.Shared.Services;

public interface IEnvoyProxyService
{
    Task<Panel[]> GetPanelsAsync();
    Task<LiveData> GetLiveDataAsync();
    Task<StreamResponse> EnableStreamAsync();
    Task<bool> TestConnectionAsync();
}