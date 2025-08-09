using System.Text.Json.Serialization;

namespace MauiEnphaseMonitor.Shared.Models;

public record Panel(
    [property: JsonPropertyName("serialNumber")] string SerialNumber,
    [property: JsonPropertyName("lastReportDate")] long LastReportDate,
    [property: JsonPropertyName("devType")] int DevType,
    [property: JsonPropertyName("lastReportWatts")] int LastReportWatts,
    [property: JsonPropertyName("maxReportWatts")] int MaxReportWatts
);

public record LiveData(
    [property: JsonPropertyName("meters")] MetersData? Meters,
    [property: JsonPropertyName("connection")] ConnectionData? Connection
);

public record MetersData(
    [property: JsonPropertyName("load")] LoadData? Load,
    [property: JsonPropertyName("pv")] PvData? Pv,
    [property: JsonPropertyName("last_update")] long? LastUpdate,
    [property: JsonPropertyName("main_relay_state")] long MainRelayState
);

public record LoadData([property: JsonPropertyName("agg_p_mw")] int AggPMw);

public record PvData([property: JsonPropertyName("agg_p_mw")] int AggPMw);

public record ConnectionData(
    [property: JsonPropertyName("sc_stream")] string? ScStream
);

public record StreamResponse(
    [property: JsonPropertyName("sc_stream")] string? ScStream
);

public record SystemDataPoint(
    DateTime Timestamp,
    double Production,
    double Consumption,
    double Net,
    int PanelTotal
);

public record PanelDataPoint(
    DateTime Timestamp,
    int Watts,
    int MaxWatts,
    long LastReportDate
);

public record EnergyMetrics(
    double Production,
    double Consumption,
    double Net,
    int PanelTotal,
    double Efficiency,
    DateTime LastUpdate,
    bool IsLiveDataEnabled,
    bool GridStatus
);