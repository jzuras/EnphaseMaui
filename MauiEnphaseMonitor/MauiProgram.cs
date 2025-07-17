using MauiEnphaseMonitor.Services;
using MauiEnphaseMonitor.Shared.Services;
using Microsoft.Extensions.Logging;

namespace MauiEnphaseMonitor;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Add device-specific services used by the MauiEnphaseMonitor.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddScoped<ITokenStorage, MauiTokenStorage>();
        builder.Services.AddScoped<IClipboardService, MauiClipboardService>();
        builder.Services.AddTransient<IEnvoyHttpClientFactory, MauiHttpClientFactory>();
        
        // Register EnvoyLocalApiService as TRANSIENT to ensure fresh HttpClient on each request
        builder.Services.AddTransient<IEnvoyLocalApiService, EnvoyLocalApiService>();

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
