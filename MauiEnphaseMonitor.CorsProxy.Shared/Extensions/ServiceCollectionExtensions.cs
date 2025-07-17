using Microsoft.Extensions.DependencyInjection;
using MauiEnphaseMonitor.CorsProxy.Shared.Services;
using MauiEnphaseMonitor.CorsProxy.Shared.Configuration;

namespace MauiEnphaseMonitor.CorsProxy.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEnvoyProxyServices(this IServiceCollection services)
    {
        services.AddSingleton<ITokenService, TokenService>();
        
        services.AddHttpClient<IEnvoyProxyService, EnvoyProxyService>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });
            
        return services;
    }
    
    public static IServiceCollection AddCorsProxyApi(this IServiceCollection services, Action<ProxyConfiguration>? configureOptions = null)
    {
        var config = new ProxyConfiguration();
        configureOptions?.Invoke(config);
        
        services.AddSingleton(config);
        
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });
        
        return services;
    }
}