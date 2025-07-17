using Microsoft.JSInterop;
using MauiEnphaseMonitor.Shared.Services;

namespace MauiEnphaseMonitor.Wasm;

public class WebClipboardService : IClipboardService
{
    private readonly IJSRuntime _jsRuntime;

    public WebClipboardService(IJSRuntime jsRuntime)
    {
        this._jsRuntime = jsRuntime;
    }

    public async Task SetTextAsync(string text)
    {
        try
        {
            // Check if JavaScript interop is available (not during prerendering)
            if (!IsJavaScriptAvailable())
            {
                throw new InvalidOperationException("JavaScript interop not available");
            }
            
            await this._jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting clipboard text: {ex.Message}");
            throw;
        }
    }

    public async Task<string?> GetTextAsync()
    {
        try
        {
            // Check if JavaScript interop is available (not during prerendering)
            if (!IsJavaScriptAvailable())
            {
                return null;
            }
            
            return await this._jsRuntime.InvokeAsync<string?>("navigator.clipboard.readText");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting clipboard text: {ex.Message}");
            return null;
        }
    }

    private bool IsJavaScriptAvailable()
    {
        // During prerendering, JSRuntime is IJSInProcessRuntime but interop isn't available
        return this._jsRuntime is IJSInProcessRuntime;
    }
}