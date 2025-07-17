using Microsoft.JSInterop;
using MauiEnphaseMonitor.Shared.Services;

namespace MauiEnphaseMonitor.Wasm;

public class WebTokenStorage : ITokenStorage
{
    private readonly IJSRuntime _jsRuntime;
    private const string TokenKey = "EnphaseToken";

    public WebTokenStorage(IJSRuntime jsRuntime)
    {
        this._jsRuntime = jsRuntime;
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            // Check if JavaScript interop is available (not during prerendering)
            if (!IsJavaScriptAvailable())
            {
                return null;
            }
            
            return await this._jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting token from localStorage: {ex.Message}");
            return null;
        }
    }

    public async Task SetTokenAsync(string token)
    {
        try
        {
            // Check if JavaScript interop is available (not during prerendering)
            if (!IsJavaScriptAvailable())
            {
                return;
            }
            
            await this._jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting token in localStorage: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> HasValidTokenAsync()
    {
        var token = await this.GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    public async Task ClearTokenAsync()
    {
        try
        {
            // Check if JavaScript interop is available (not during prerendering)
            if (!IsJavaScriptAvailable())
            {
                return;
            }
            
            await this._jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing token from localStorage: {ex.Message}");
            throw;
        }
    }

    private bool IsJavaScriptAvailable()
    {
        // During prerendering, JSRuntime is IJSInProcessRuntime but interop isn't available
        // We can check if we're in a browser context by trying to access the IJSInProcessRuntime
        return this._jsRuntime is IJSInProcessRuntime;
    }
}