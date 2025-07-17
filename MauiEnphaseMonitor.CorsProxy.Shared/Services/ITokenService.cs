namespace MauiEnphaseMonitor.CorsProxy.Shared.Services;

public interface ITokenService
{
    void SetToken(string token);
    string? GetToken();
    bool HasValidToken();
    DateTime? GetLastUpdated();
    void ClearToken();
}