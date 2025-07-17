using MauiEnphaseMonitor.Shared.Services;

namespace MauiEnphaseMonitor.Services;

public class MauiTokenStorage : ITokenStorage
{
    private const string TokenKey = "EnphaseToken";

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var token = await SecureStorage.GetAsync(TokenKey);
            var preview = string.IsNullOrEmpty(token) ? "NONE" : 
                token.Length > 10 ? $"{token[..6]}...{token[^4..]}" : "short";
            System.Diagnostics.Debug.WriteLine($"MauiTokenStorage.GetTokenAsync: {preview} [{token?.Length ?? 0} chars]");
            return token;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting token from SecureStorage: {ex.Message}");
            return null;
        }
    }

    public async Task SetTokenAsync(string token)
    {
        try
        {
            var preview = token.Length > 10 ? $"{token[..6]}...{token[^4..]}" : "short";
            System.Diagnostics.Debug.WriteLine($"MauiTokenStorage.SetTokenAsync: {preview} [{token.Length} chars]");
            await SecureStorage.SetAsync(TokenKey, token);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting token in SecureStorage: {ex.Message}");
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
            SecureStorage.Remove(TokenKey);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing token from SecureStorage: {ex.Message}");
            throw;
        }
    }
}