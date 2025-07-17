namespace MauiEnphaseMonitor.Shared.Services;

public interface ITokenStorage
{
    Task<string?> GetTokenAsync();
    Task SetTokenAsync(string token);
    Task<bool> HasValidTokenAsync();
    Task ClearTokenAsync();
}