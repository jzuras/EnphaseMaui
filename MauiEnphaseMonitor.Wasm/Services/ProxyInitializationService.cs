using System.Text;
using System.Text.Json;
using MauiEnphaseMonitor.Shared.Services;

namespace MauiEnphaseMonitor.Wasm;

public class ProxyInitializationService : IProxyInitializationService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenStorage _tokenStorage;
    private const string ProxyBaseUrl = "http://localhost:8080";

    public ProxyInitializationService(HttpClient httpClient, ITokenStorage tokenStorage)
    {
        _httpClient = httpClient;
        _tokenStorage = tokenStorage;
    }

    public async Task<bool> InitializeProxyAsync()
    {
        try
        {
            var token = await _tokenStorage.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            var tokenRequest = new { Token = token };
            var jsonContent = JsonSerializer.Serialize(tokenRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{ProxyBaseUrl}/api/token", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsProxyAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{ProxyBaseUrl}/api/token/status");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetProxyStatusAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{ProxyBaseUrl}/api/token/status");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var statusResponse = JsonSerializer.Deserialize<JsonElement>(json);
                return statusResponse.GetProperty("status").GetString() ?? "Unknown";
            }
            return "Proxy not available";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}