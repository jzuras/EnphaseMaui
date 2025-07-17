using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MauiEnphaseMonitor.CorsProxy.Shared.Models;
using MauiEnphaseMonitor.CorsProxy.Shared.Services;

namespace MauiEnphaseMonitor.CorsProxy.Shared.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapEnvoyProxyEndpoints(this WebApplication app)
    {
        // Token management endpoints
        app.MapPost("/api/token", (TokenRequest request, ITokenService tokenService) =>
        {
            try
            {
                tokenService.SetToken(request.Token);
                return Results.Ok(new TokenResponse(true, "Token set successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new TokenResponse(false, ex.Message));
            }
        });

        app.MapGet("/api/token/status", (ITokenService tokenService) =>
        {
            return Results.Ok(new ProxyStatusResponse(
                tokenService.HasValidToken(),
                tokenService.GetLastUpdated(),
                tokenService.HasValidToken() ? "Ready" : "Waiting for token"
            ));
        });

        app.MapDelete("/api/token", (ITokenService tokenService) =>
        {
            tokenService.ClearToken();
            return Results.Ok(new TokenResponse(true, "Token cleared"));
        });

        // Envoy proxy endpoints
        app.MapGet("/api/envoy/panels", async (IEnvoyProxyService proxyService) =>
        {
            try
            {
                var panels = await proxyService.GetPanelsAsync();
                return Results.Ok(panels);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error fetching panels: {ex.Message}");
            }
        });

        app.MapGet("/api/envoy/livedata", async (IEnvoyProxyService proxyService) =>
        {
            try
            {
                var liveData = await proxyService.GetLiveDataAsync();
                return Results.Ok(liveData);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error fetching live data: {ex.Message}");
            }
        });

        app.MapPost("/api/envoy/stream", async (IEnvoyProxyService proxyService) =>
        {
            try
            {
                var result = await proxyService.EnableStreamAsync();
                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error enabling stream: {ex.Message}");
            }
        });

        app.MapGet("/api/envoy/test", async (IEnvoyProxyService proxyService) =>
        {
            try
            {
                var isConnected = await proxyService.TestConnectionAsync();
                return Results.Ok(new { connected = isConnected });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error testing connection: {ex.Message}");
            }
        });

        return app;
    }
}