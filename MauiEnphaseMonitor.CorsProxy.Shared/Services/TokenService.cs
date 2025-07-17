namespace MauiEnphaseMonitor.CorsProxy.Shared.Services;

public class TokenService : ITokenService
{
    private string? _token;
    private DateTime? _lastUpdated;
    private readonly object _lock = new();

    public void SetToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));

        lock (_lock)
        {
            _token = token;
            _lastUpdated = DateTime.UtcNow;
        }
    }

    public string? GetToken()
    {
        lock (_lock)
        {
            return _token;
        }
    }

    public bool HasValidToken()
    {
        lock (_lock)
        {
            return !string.IsNullOrWhiteSpace(_token);
        }
    }

    public DateTime? GetLastUpdated()
    {
        lock (_lock)
        {
            return _lastUpdated;
        }
    }

    public void ClearToken()
    {
        lock (_lock)
        {
            _token = null;
            _lastUpdated = null;
        }
    }
}