using MauiEnphaseMonitor.Shared.Services;

namespace MauiEnphaseMonitor.Services;

public class MauiClipboardService : IClipboardService
{
    public async Task SetTextAsync(string text)
    {
        try
        {
            await Clipboard.SetTextAsync(text);
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
            return await Clipboard.GetTextAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting clipboard text: {ex.Message}");
            return null;
        }
    }
}