using System.Text.Json.Serialization;

namespace MauiEnphaseMonitor.CorsProxy.Shared.Models;

public record LiveData(
    [property: JsonPropertyName("meters")] MetersData? Meters,
    [property: JsonPropertyName("connection")] ConnectionData? Connection
);

public record MetersData(
    [property: JsonPropertyName("load")] LoadData? Load,
    [property: JsonPropertyName("pv")] PvData? Pv,
    [property: JsonPropertyName("last_update")] long? LastUpdate
);

public record LoadData([property: JsonPropertyName("agg_p_mw")] int AggPMw);

public record PvData([property: JsonPropertyName("agg_p_mw")] int AggPMw);

public record ConnectionData(
    [property: JsonPropertyName("sc_stream")] string? ScStream
);

public record StreamResponse(
    [property: JsonPropertyName("sc_stream")] string? ScStream
);