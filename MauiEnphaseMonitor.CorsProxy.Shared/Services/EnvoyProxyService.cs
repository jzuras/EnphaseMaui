using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MauiEnphaseMonitor.CorsProxy.Shared.Models;

namespace MauiEnphaseMonitor.CorsProxy.Shared.Services;

public class EnvoyProxyService : IEnvoyProxyService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenService _tokenService;

    public EnvoyProxyService(HttpClient httpClient, ITokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
    }

    public async Task<Panel[]> GetPanelsAsync()
    {
        EnsureTokenAvailable();
        SetAuthorizationHeader();

        var response = await _httpClient.GetAsync("https://envoy.local/api/v1/production/inverters");
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        var panels = JsonSerializer.Deserialize<Panel[]>(jsonString);
        
        return panels ?? Array.Empty<Panel>();
    }

    public async Task<LiveData> GetLiveDataAsync()
    {
        EnsureTokenAvailable();
        SetAuthorizationHeader();

        var response = await _httpClient.GetAsync("https://envoy.local/ivp/livedata/status");
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        var liveData = JsonSerializer.Deserialize<LiveData>(jsonString);
        
        return liveData ?? new LiveData(null, null);
    }

    public async Task<StreamResponse> EnableStreamAsync()
    {
        EnsureTokenAvailable();
        SetAuthorizationHeader();

        var content = new StringContent("{}", Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("https://envoy.local/ivp/livedata/stream", content);
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        var streamResponse = JsonSerializer.Deserialize<StreamResponse>(jsonString);
        
        return streamResponse ?? new StreamResponse(null);
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            EnsureTokenAvailable();
            SetAuthorizationHeader();

            var response = await _httpClient.GetAsync("https://envoy.local/ivp/livedata/status");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private void EnsureTokenAvailable()
    {
        if (!_tokenService.HasValidToken())
        {
            throw new UnauthorizedAccessException("No valid token available. Please initialize the proxy with a token first.");
        }
    }

    private void SetAuthorizationHeader()
    {
        var token = _tokenService.GetToken();
        if (token != null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}