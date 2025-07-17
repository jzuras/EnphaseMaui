using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MauiEnphaseMonitor.Shared.Models;
using MauiEnphaseMonitor.Shared.Services;

namespace MauiEnphaseMonitor.Shared.Components.Pages;

public partial class Monitor : ComponentBase, IDisposable
{
    [Inject] public IEnvoyLocalApiService EnvoyService { get; set; } = null!;
    [Inject] public ITokenStorage TokenStorage { get; set; } = null!;
    [Inject] public IServiceProvider ServiceProvider { get; set; } = null!;

    private EnergyMetrics? currentMetrics;
    private bool showTokenMessage = false;
    private bool isLoading = false;
    private string errorMessage = string.Empty;
    private string debugMessage = string.Empty;
    private int refreshInterval = 15;
    private bool disposed = false;
    private bool hasInitialized = false;

    protected override async Task OnInitializedAsync()
    {
        await CheckTokenAndLoadDataAsync();
        this.hasInitialized = true;
    }

    protected override async Task OnParametersSetAsync()
    {
        // Only refresh if we've already initialized (i.e., this is a navigation back)
        if (this.hasInitialized)
        {
            await CheckTokenAndLoadDataAsync();
        }
    }

    private async Task CheckTokenAndLoadDataAsync()
    {
        string? token = null;
        try
        {
            // NUCLEAR OPTION: Dispose current service and get a fresh one to kill any timers
            if (this.EnvoyService is IDisposable disposableService)
            {
                System.Diagnostics.Debug.WriteLine($"DISPOSING old service {this.EnvoyService.GetHashCode()}");
                this.EnvoyService.EnergyMetricsUpdated -= OnEnergyMetricsUpdated;
                disposableService.Dispose();
            }
            
            // Get a completely fresh service instance
            this.EnvoyService = this.ServiceProvider.GetRequiredService<IEnvoyLocalApiService>();
            System.Diagnostics.Debug.WriteLine($"CREATED fresh service {this.EnvoyService.GetHashCode()}");
            
            token = await this.TokenStorage.GetTokenAsync();
            
            if (string.IsNullOrEmpty(token))
            {
                this.showTokenMessage = true;
                this.debugMessage = string.Empty; // Clear debug message when no token
                return;
            }

            // For web projects, initialize the CORS proxy with the token
            await TryInitializeProxyAsync();

            await LoadInitialDataAsync();
            
            // Only start auto-refresh and subscribe if manual load was successful
            if (this.currentMetrics is not null)
            {
                this.EnvoyService.EnergyMetricsUpdated += OnEnergyMetricsUpdated;
                this.EnvoyService.StartAutoRefresh(this.refreshInterval);
            }
        }
        catch (Exception ex)
        {
            this.errorMessage = $"Error checking token: {ex.Message}";
            var tokenPreview = string.IsNullOrEmpty(token) ? "NONE" : 
                token.Length > 10 ? $"{token[..6]}...{token[^4..]}" : "short";
            this.debugMessage = $"Service: {this.EnvoyService.GetHashCode()} | Token: {(string.IsNullOrEmpty(token) ? "NONE" : $"[{token.Length} chars] ({tokenPreview})")} | Init error: {ex.Message}";
            this.showTokenMessage = true;
        }
        finally
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task TryInitializeProxyAsync()
    {
        try
        {
            // Check if we're in a web project by looking for the proxy service
            var proxyService = this.ServiceProvider.GetService<IProxyInitializationService>();
            if (proxyService != null)
            {
                var success = await proxyService.InitializeProxyAsync();
                
                if (!success)
                {
                    var status = await proxyService.GetProxyStatusAsync();
                    this.debugMessage = $"Proxy initialization failed | Status: {status}";
                }
            }
            // No debug message for successful proxy init or MAUI mode
        }
        catch (Exception ex)
        {
            this.debugMessage = $"Proxy initialization error: {ex.Message}";
        }
    }

    private async Task LoadInitialDataAsync()
    {
        try
        {
            this.isLoading = true;
            this.errorMessage = string.Empty;
            
            System.Diagnostics.Debug.WriteLine($"MONITOR: About to call GetCurrentEnergyMetricsAsync on service {this.EnvoyService.GetHashCode()}");
            var metrics = await this.EnvoyService.GetCurrentEnergyMetricsAsync();
            System.Diagnostics.Debug.WriteLine($"MONITOR: GetCurrentEnergyMetricsAsync returned: {(metrics is not null ? "SUCCESS" : "NULL")}");
            System.Diagnostics.Debug.WriteLine($"MONITOR: Service LastErrorDetails: {this.EnvoyService.LastErrorDetails}");
            
            if (metrics is not null)
            {
                this.currentMetrics = metrics;
                this.showTokenMessage = false;
                this.debugMessage = string.Empty; // Clear debug message on success
            }
            else
            {
                this.errorMessage = "Unable to fetch energy data. Please check your connection to the Envoy device.";
                this.debugMessage = $"No data received | Service Details: {this.EnvoyService.LastErrorDetails}";
            }
        }
        catch (Exception ex)
        {
            this.errorMessage = $"Error loading data: {ex.Message}";
            this.debugMessage = $"Exception: {ex.Message} | Service Details: {this.EnvoyService.LastErrorDetails}";
        }
        finally
        {
            this.isLoading = false;
        }
    }

    private async Task RefreshNow()
    {
        try
        {
            var token = await this.TokenStorage.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                this.showTokenMessage = true;
                this.debugMessage = string.Empty;
                return;
            }

            await LoadInitialDataAsync();
        }
        catch (Exception ex)
        {
            this.errorMessage = $"Error refreshing data: {ex.Message}";
            this.debugMessage = $"Refresh error: {ex.Message}";
        }
        finally
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    private void SetRefreshInterval(int intervalSeconds)
    {
        try
        {
            this.refreshInterval = intervalSeconds;
            this.EnvoyService.SetRefreshInterval(intervalSeconds);
        }
        catch (Exception ex)
        {
            this.errorMessage = $"Error setting refresh interval: {ex.Message}";
            this.debugMessage = $"Interval error: {ex.Message}";
        }
        finally
        {
            InvokeAsync(StateHasChanged);
        }
    }

    private void OnEnergyMetricsUpdated(object? sender, EnergyMetrics metrics)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"MONITOR: OnEnergyMetricsUpdated called with P:{metrics.Production:F1}W - overriding manual results!");
            this.currentMetrics = metrics;
            this.showTokenMessage = false;
            this.errorMessage = string.Empty;
            this.debugMessage = string.Empty; // Clear debug message on successful update
        }
        catch (Exception ex)
        {
            this.errorMessage = $"Error updating metrics: {ex.Message}";
            this.debugMessage = $"Update error: {ex.Message}";
        }
        finally
        {
            InvokeAsync(StateHasChanged);
        }
    }

    private string GetNetValueClass()
    {
        if (this.currentMetrics is null)
        {
            return "net-neutral";
        }

        return this.currentMetrics.Net switch
        {
            > 0.1 => "net-positive",
            < -0.1 => "net-negative",
            _ => "net-neutral"
        };
    }

    private string GetNetValueDisplay()
    {
        if (this.currentMetrics is null)
        {
            return "0 W";
        }

        var sign = this.currentMetrics.Net >= 0 ? "+" : "";
        return $"{sign}{this.currentMetrics.Net:F0} W";
    }

    public void Dispose()
    {
        if (!this.disposed)
        {
            try
            {
                this.EnvoyService.EnergyMetricsUpdated -= OnEnergyMetricsUpdated;
                this.EnvoyService.StopAutoRefresh();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing Monitor component: {ex.Message}");
            }
            
            this.disposed = true;
        }
    }
}