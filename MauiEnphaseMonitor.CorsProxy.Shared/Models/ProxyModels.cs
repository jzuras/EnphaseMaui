namespace MauiEnphaseMonitor.CorsProxy.Shared.Models;

public record TokenRequest(string Token);

public record TokenResponse(bool Success, string? Message = null);

public record ProxyStatusResponse(
    bool HasToken,
    DateTime? LastTokenUpdate,
    string Status
);