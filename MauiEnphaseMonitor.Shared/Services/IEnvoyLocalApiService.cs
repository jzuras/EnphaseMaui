using MauiEnphaseMonitor.Shared.Models;

namespace MauiEnphaseMonitor.Shared.Services;

public interface IEnvoyLocalApiService
{
    string LastErrorDetails { get; }
    
    void ClearAuthenticationCache();
    Task<EnergyMetrics?> GetCurrentEnergyMetricsAsync();
    Task<Panel[]?> GetPanelDataAsync();
    Task<LiveData?> GetLiveDataAsync();
    Task<bool> EnableLiveDataStreamAsync();
    Task<bool> IsLiveDataStreamEnabledAsync();
    void StartAutoRefresh(int intervalSeconds = 15);
    void StopAutoRefresh();
    void SetRefreshInterval(int intervalSeconds);
    event EventHandler<EnergyMetrics>? EnergyMetricsUpdated;
}