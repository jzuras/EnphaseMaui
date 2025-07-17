using MauiEnphaseMonitor.Shared.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace MauiEnphaseMonitor.Wasm;

public class Program
{
    //public static async Task Main(string[] args)
    //{
    //    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    //    builder.RootComponents.Add<App>("#app");
    //    builder.RootComponents.Add<HeadOutlet>("head::after");

    //    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

    //    await builder.Build().RunAsync();
    //}


    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        // Add device-specific services used by the MauiEnphaseMonitor.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddScoped<ITokenStorage, WebTokenStorage>();
        builder.Services.AddScoped<IClipboardService, WebClipboardService>();
        builder.Services.AddTransient<IEnvoyHttpClientFactory, WebHttpClientFactory>();

        // Register EnvoyLocalApiService as TRANSIENT to ensure fresh HttpClient
        builder.Services.AddTransient<IEnvoyLocalApiService, EnvoyLocalApiService>();

        // Add proxy initialization service
        builder.Services.AddScoped<IProxyInitializationService, ProxyInitializationService>();
        builder.Services.AddScoped<HttpClient>();

        await builder.Build().RunAsync();
    }
}
