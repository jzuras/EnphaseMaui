using System.Text.Json.Serialization;

namespace MauiEnphaseMonitor.CorsProxy.Shared.Models;

public record Panel(
    [property: JsonPropertyName("serialNumber")] string SerialNumber,
    [property: JsonPropertyName("lastReportDate")] long LastReportDate,
    [property: JsonPropertyName("devType")] int DevType,
    [property: JsonPropertyName("lastReportWatts")] int LastReportWatts,
    [property: JsonPropertyName("maxReportWatts")] int MaxReportWatts
);