using MauiEnphaseMonitor.Shared.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MauiEnphaseMonitor.Shared.Services;

public class EnvoyLocalApiService : IEnvoyLocalApiService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ITokenStorage _tokenStorage;
    private readonly Timer _refreshTimer;
    private int _refreshInterval = 15;
    private double _lastProduction = 0;
    private double _lastConsumption = 0;
    private int _lastPanelTotal = 0;
    private bool _disposed = false;
    private bool _manualRefreshInProgress = false;
    private bool _timerStopped = false;

    public string LastErrorDetails { get; private set; } = string.Empty;
    public event EventHandler<EnergyMetrics>? EnergyMetricsUpdated;

    public void ClearAuthenticationCache()
    {
        var oldAuth = this._httpClient.DefaultRequestHeaders.Authorization?.Parameter;
        var oldPreview = string.IsNullOrEmpty(oldAuth) ? "NONE" : 
            oldAuth.Length > 10 ? $"{oldAuth[..6]}...{oldAuth[^4..]}" : "short";
            
        this._httpClient.DefaultRequestHeaders.Authorization = null;
        this.LastErrorDetails = $"Auth cache cleared (was: {oldPreview})";
        
        System.Diagnostics.Debug.WriteLine($"EnvoyService.ClearAuthenticationCache: Cleared {oldPreview}");
    }

    public EnvoyLocalApiService(ITokenStorage tokenStorage, IEnvoyHttpClientFactory httpClientFactory)
    {
        this._tokenStorage = tokenStorage;
        this._httpClient = httpClientFactory.CreateEnvoyClient();
        this._refreshTimer = new Timer(OnRefreshTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
        
        System.Diagnostics.Debug.WriteLine($"EnvoyLocalApiService {this.GetHashCode()} created with new HttpClient");
    }

    public async Task<EnergyMetrics?> GetCurrentEnergyMetricsAsync()
    {
        try
        {
            // Set flag to prevent timer interference during manual calls
            this._manualRefreshInProgress = true;
            
            this.LastErrorDetails = $"Service Instance: {this.GetHashCode()} | MANUAL CALL";
            
            var token = await this._tokenStorage.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                this.LastErrorDetails += " | No token available";
                return null;
            }

            var tokenPreview = token.Length > 10 ? $"{token[..6]}...{token[^4..]}" : "short";
            this.LastErrorDetails += $" | Token [{token.Length} chars] ({tokenPreview})";
            
            // Clear and set authorization header to ensure new token is used
            var oldAuth = this._httpClient.DefaultRequestHeaders.Authorization?.Parameter;
            var oldAuthPreview = string.IsNullOrEmpty(oldAuth) ? "NONE" : 
                oldAuth.Length > 10 ? $"{oldAuth[..6]}...{oldAuth[^4..]}" : "short";
            
            this._httpClient.DefaultRequestHeaders.Authorization = null;
            this._httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
                
            this.LastErrorDetails += $" | Old Auth: ({oldAuthPreview}) | New Auth: ({tokenPreview})";

            this.LastErrorDetails += " | Starting API calls";
            var panelsTask = GetPanelDataAsync();
            var liveDataTask = GetLiveDataAsync();

            await Task.WhenAll(panelsTask, liveDataTask);

            var panels = await panelsTask;
            var liveData = await liveDataTask;

            this.LastErrorDetails += $" | Panels: {panels?.Length ?? 0}, Live: {(liveData?.Meters is not null ? "OK" : "NULL")}";

            if (liveData?.Meters is null)
            {
                this.LastErrorDetails += " | No live data meters";
                return null;
            }

            var production = (liveData.Meters.Pv?.AggPMw ?? 0) / 1000.0; // Convert milliwatts to watts
            var consumption = (liveData.Meters.Load?.AggPMw ?? 0) / 1000.0; // Convert milliwatts to watts
            var net = production - consumption;
            var panelTotal = panels?.Sum(p => p.LastReportWatts) ?? 0;
            var maxTotal = panels?.Sum(p => p.MaxReportWatts) ?? 1;
            var efficiency = maxTotal > 0 ? panelTotal * 100.0 / maxTotal : 0;
            var isLiveDataEnabled = liveData.Connection?.ScStream == "enabled";

            //Grid Status Properties:
            //-meters.main_relay_state - State of the main relay(0 = open / off - grid, other values = closed / on - grid)
            //-meters.gen_relay_state - State of the generator relay
            //-meters.grid.agg_p_mw - Grid power flow(0 = no grid connection, positive = importing from grid, negative = exporting to grid)
            var gridStatus = liveData.Meters.MainRelayState == 0 ? false : true;

            this.LastErrorDetails += $" | SUCCESS P:{production:F1}W C:{consumption:F1}W";

            return new EnergyMetrics(
                production,
                consumption,
                net,
                panelTotal,
                efficiency,
                DateTime.UtcNow,
                isLiveDataEnabled,
                gridStatus
            );
        }
        catch (Exception ex)
        {
            this.LastErrorDetails = $"Exception: {ex.GetType().Name} - {ex.Message}";
            if (ex.InnerException is not null)
            {
                this.LastErrorDetails += $" | Inner: {ex.InnerException.Message}";
            }
            return null;
        }
        finally
        {
            this._manualRefreshInProgress = false;
        }
    }

    private async Task<EnergyMetrics?> GetCurrentEnergyMetricsForTimerAsync()
    {
        try
        {
            this.LastErrorDetails = $"Service Instance: {this.GetHashCode()} | TIMER CALL";
            
            var token = await this._tokenStorage.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                this.LastErrorDetails += " | No token available";
                return null;
            }

            var tokenPreview = token.Length > 10 ? $"{token[..6]}...{token[^4..]}" : "short";
            this.LastErrorDetails += $" | Token [{token.Length} chars] ({tokenPreview})";
            
            // Clear and set authorization header to ensure new token is used
            var oldAuth = this._httpClient.DefaultRequestHeaders.Authorization?.Parameter;
            var oldAuthPreview = string.IsNullOrEmpty(oldAuth) ? "NONE" : 
                oldAuth.Length > 10 ? $"{oldAuth[..6]}...{oldAuth[^4..]}" : "short";
            
            this._httpClient.DefaultRequestHeaders.Authorization = null;
            this._httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
                
            this.LastErrorDetails += $" | Old Auth: ({oldAuthPreview}) | New Auth: ({tokenPreview})";

            this.LastErrorDetails += " | Starting API calls";
            var panelsTask = GetPanelDataAsync();
            var liveDataTask = GetLiveDataAsync();

            await Task.WhenAll(panelsTask, liveDataTask);

            var panels = await panelsTask;
            var liveData = await liveDataTask;

            this.LastErrorDetails += $" | Panels: {panels?.Length ?? 0}, Live: {(liveData?.Meters is not null ? "OK" : "NULL")}";

            if (liveData?.Meters is null)
            {
                this.LastErrorDetails += " | No live data meters";
                return null;
            }

            var production = (liveData.Meters.Pv?.AggPMw ?? 0) / 1000.0; // Convert milliwatts to watts
            var consumption = (liveData.Meters.Load?.AggPMw ?? 0) / 1000.0; // Convert milliwatts to watts
            var net = production - consumption;
            var panelTotal = panels?.Sum(p => p.LastReportWatts) ?? 0;
            var maxTotal = panels?.Sum(p => p.MaxReportWatts) ?? 1;
            var efficiency = maxTotal > 0 ? panelTotal * 100.0 / maxTotal : 0;
            var isLiveDataEnabled = liveData.Connection?.ScStream == "enabled";

            //Grid Status Properties:
            //-meters.main_relay_state - State of the main relay(0 = open / off - grid, other values = closed / on - grid)
            //-meters.gen_relay_state - State of the generator relay
            //-meters.grid.agg_p_mw - Grid power flow(0 = no grid connection, positive = importing from grid, negative = exporting to grid)
            var gridStatus = liveData.Meters.MainRelayState == 0 ? false : true;

            this.LastErrorDetails += $" | SUCCESS P:{production:F1}W C:{consumption:F1}W";

            return new EnergyMetrics(
                production,
                consumption,
                net,
                panelTotal,
                efficiency,
                DateTime.UtcNow,
                isLiveDataEnabled,
                gridStatus
            );
        }
        catch (Exception ex)
        {
            this.LastErrorDetails = $"Timer Exception: {ex.GetType().Name} - {ex.Message}";
            if (ex.InnerException is not null)
            {
                this.LastErrorDetails += $" | Inner: {ex.InnerException.Message}";
            }
            return null;
        }
    }

    public async Task<Panel[]?> GetPanelDataAsync()
    {
        try
        {
            var endpoint = IsUsingProxy() ? "panels" : "api/v1/production/inverters";
            var response = await this._httpClient.GetAsync(endpoint);
            this.LastErrorDetails += $" | Panels HTTP: {(int)response.StatusCode} {response.StatusCode}";
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                this.LastErrorDetails += $" | Panel Error: {errorContent.Substring(0, Math.Min(100, errorContent.Length))}";
                return null;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            this.LastErrorDetails += $" | Panel JSON: {json.Length} chars";
            
            var panels = JsonSerializer.Deserialize<Panel[]>(json);
            return panels;
        }
        catch (Exception ex)
        {
            this.LastErrorDetails += $" | Panel Exception: {ex.GetType().Name} - {ex.Message}";
            return null;
        }
    }

    public async Task<LiveData?> GetLiveDataAsync()
    {
        try
        {
            var endpoint = IsUsingProxy() ? "livedata" : "ivp/livedata/status";
            var response = await this._httpClient.GetAsync(endpoint);
            this.LastErrorDetails += $" | Live HTTP: {(int)response.StatusCode} {response.StatusCode}";
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                this.LastErrorDetails += $" | Live Error: {errorContent.Substring(0, Math.Min(100, errorContent.Length))}";
                return null;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            this.LastErrorDetails += $" | Live JSON: {json.Length} chars";
            
            var liveData = JsonSerializer.Deserialize<LiveData>(json);
            return liveData;
        }
        catch (Exception ex)
        {
            this.LastErrorDetails += $" | Live Exception: {ex.GetType().Name} - {ex.Message}";
            return null;
        }
    }

    public async Task<bool> EnableLiveDataStreamAsync()
    {
        try
        {
            var enableRequest = new { enable = 1 };
            var jsonContent = JsonSerializer.Serialize(enableRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var endpoint = IsUsingProxy() ? "stream" : "ivp/livedata/stream";
            var response = await this._httpClient.PostAsync(endpoint, content);

            if (response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync();
                var streamResponse = JsonSerializer.Deserialize<StreamResponse>(responseText);
                return streamResponse?.ScStream == "enabled";
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enabling live data stream: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> IsLiveDataStreamEnabledAsync()
    {
        try
        {
            var liveData = await GetLiveDataAsync();
            return liveData?.Connection?.ScStream == "enabled";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking live data stream status: {ex.Message}");
            return false;
        }
    }

    public void StartAutoRefresh(int intervalSeconds = 15)
    {
        System.Diagnostics.Debug.WriteLine($"StartAutoRefresh({intervalSeconds}s) called on service {this.GetHashCode()}");
        this._timerStopped = false;
        this._refreshInterval = intervalSeconds;
        this._refreshTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(intervalSeconds));
    }

    public void StopAutoRefresh()
    {
        System.Diagnostics.Debug.WriteLine($"StopAutoRefresh called on service {this.GetHashCode()}");
        this._timerStopped = true;
        this._refreshTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    public void SetRefreshInterval(int intervalSeconds)
    {
        this._refreshInterval = intervalSeconds;
        this._refreshTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(intervalSeconds));
    }

    private async void OnRefreshTimerElapsed(object? state)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"TIMER CALLBACK: Starting on service {this.GetHashCode()}");
            
            // Check if timer was stopped - abort immediately
            if (this._timerStopped)
            {
                System.Diagnostics.Debug.WriteLine($"TIMER CALLBACK: Timer was stopped, aborting callback");
                return;
            }
            
            // Don't interfere if manual refresh is in progress
            if (this._manualRefreshInProgress)
            {
                System.Diagnostics.Debug.WriteLine($"TIMER CALLBACK: Manual refresh in progress, skipping timer callback");
                return;
            }
            
            // Check what token the timer thinks it should use
            var timerToken = await this._tokenStorage.GetTokenAsync();
            var timerPreview = string.IsNullOrEmpty(timerToken) ? "NONE" : 
                timerToken.Length > 10 ? $"{timerToken[..6]}...{timerToken[^4..]}" : "short";
            System.Diagnostics.Debug.WriteLine($"TIMER CALLBACK: Token from storage: [{timerToken?.Length ?? 0} chars] ({timerPreview})");
            
            // Check again if timer was stopped during token fetch
            if (this._timerStopped)
            {
                System.Diagnostics.Debug.WriteLine($"TIMER CALLBACK: Timer was stopped during execution, aborting");
                return;
            }
            
            // Call GetCurrentEnergyMetricsAsync but mark it as a timer call
            var metrics = await GetCurrentEnergyMetricsForTimerAsync();
            if (metrics is not null)
            {
                System.Diagnostics.Debug.WriteLine($"TIMER CALLBACK: Got metrics! P:{metrics.Production:F1}W");
                
                // Final check before firing event
                if (this._timerStopped)
                {
                    System.Diagnostics.Debug.WriteLine($"TIMER CALLBACK: Timer was stopped, NOT firing event");
                    return;
                }
                
                if (HasSignificantChange(metrics))
                {
                    this._lastProduction = metrics.Production;
                    this._lastConsumption = metrics.Consumption;
                    this._lastPanelTotal = metrics.PanelTotal;
                    
                    EnergyMetricsUpdated?.Invoke(this, metrics);
                    System.Diagnostics.Debug.WriteLine($"TIMER CALLBACK: Fired EnergyMetricsUpdated event");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"TIMER CALLBACK: No significant change");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"TIMER CALLBACK: No metrics received - should fail with bad token");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TIMER CALLBACK ERROR: {ex.Message}");
        }
    }

    private bool HasSignificantChange(EnergyMetrics metrics)
    {
        return Math.Abs(metrics.Production - this._lastProduction) > 0.01 ||
               Math.Abs(metrics.Consumption - this._lastConsumption) > 0.01 ||
               metrics.PanelTotal != this._lastPanelTotal;
    }

    private bool IsUsingProxy()
    {
        return this._httpClient.BaseAddress?.Host == "localhost";
    }

    public void Dispose()
    {
        if (!this._disposed)
        {
            this._refreshTimer?.Dispose();
            this._disposed = true;
        }
    }
}