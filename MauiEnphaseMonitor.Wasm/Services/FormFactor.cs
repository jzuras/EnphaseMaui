using MauiEnphaseMonitor.Shared.Services;

namespace MauiEnphaseMonitor.Wasm;

public class FormFactor : IFormFactor
{
    public string GetFormFactor()
    {
        return "WebAssembly";
    }

    public string GetPlatform()
    {
        return Environment.OSVersion.ToString();
    }
}
