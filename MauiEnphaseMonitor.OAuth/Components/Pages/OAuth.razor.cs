using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Text;

namespace MauiEnphaseMonitor.OAuth.Components.Pages;

public partial class OAuth
{
    [Inject]
    private IOptions<Settings> SettingsOptions { get; set; } = default!;
    
    [Inject]
    private HttpClient HttpClient { get; set; } = default!;
    
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private Settings Settings => SettingsOptions.Value;
    
    private string? EnphaseEmail { get; set; }
    private string? EnphasePassword { get; set; }
    private bool IsProcessing { get; set; } = false;
    private string? ErrorMessage { get; set; }
    private string? TokenResult { get; set; }

    private async Task GetTokenDirectly()
    {
        try
        {
            this.IsProcessing = true;
            this.ErrorMessage = null;
            this.TokenResult = null;
            this.StateHasChanged();

            // Step 1: Login to get session_id
            var loginData = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["user[email]"] = this.EnphaseEmail!,
                ["user[password]"] = this.EnphasePassword!
            });

            var loginResponse = await this.HttpClient.PostAsync("http://enlighten.enphaseenergy.com/login/login.json?", loginData);
            
            if (!loginResponse.IsSuccessStatusCode)
            {
                this.ErrorMessage = $"Login failed: {loginResponse.StatusCode}";
                return;
            }

            var loginJson = await loginResponse.Content.ReadAsStringAsync();
            using var loginDoc = JsonDocument.Parse(loginJson);
            
            if (!loginDoc.RootElement.TryGetProperty("session_id", out var sessionIdElement))
            {
                this.ErrorMessage = "Failed to get session ID from login response";
                return;
            }

            var sessionId = sessionIdElement.GetString();

            // Step 2: Get token using session_id
            var tokenData = new
            {
                session_id = sessionId,
                serial_num = this.Settings.EnphaseSerialNumber,
                username = this.EnphaseEmail
            };

            var tokenJson = JsonSerializer.Serialize(tokenData);
            var tokenContent = new StringContent(tokenJson, Encoding.UTF8, "application/json");

            var tokenResponse = await this.HttpClient.PostAsync("http://entrez.enphaseenergy.com/tokens", tokenContent);
            
            if (!tokenResponse.IsSuccessStatusCode)
            {
                this.ErrorMessage = $"Token request failed: {tokenResponse.StatusCode}";
                return;
            }

            this.TokenResult = await tokenResponse.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            this.ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            this.IsProcessing = false;
            this.StateHasChanged();
        }
    }

    private async Task CopyToClipboard(string? text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            try
            {
                var result = await this.JSRuntime.InvokeAsync<bool>("clipboardInterop.copyToClipboard", text);
                if (result)
                {
                    // Could add a toast notification here in the future
                    Console.WriteLine("Token copied to clipboard successfully");
                }
                else
                {
                    Console.WriteLine("Failed to copy token to clipboard");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying to clipboard: {ex.Message}");
            }
        }
    }
}
