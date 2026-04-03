namespace GarageFlow.Infrastructure.Sync;

public class SyncConfiguration
{
    public string ApiUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public bool SyncEnabled { get; set; }
    public int SyncIntervalSeconds { get; set; } = 60;

    public static SyncConfiguration FromEnvironment()
    {
        var deviceId = Environment.GetEnvironmentVariable("GARAGEFLOW_DEVICE_ID") ?? "auto";
        if (deviceId == "auto")
            deviceId = GetOrCreateDeviceId();

        return new SyncConfiguration
        {
            ApiUrl = Environment.GetEnvironmentVariable("GARAGEFLOW_API_URL") ?? "https://localhost:5001",
            ApiKey = Environment.GetEnvironmentVariable("GARAGEFLOW_API_KEY") ?? "",
            DeviceId = deviceId,
            SyncEnabled = bool.TryParse(Environment.GetEnvironmentVariable("GARAGEFLOW_SYNC_ENABLED"), out var enabled) && enabled,
            SyncIntervalSeconds = int.TryParse(Environment.GetEnvironmentVariable("GARAGEFLOW_SYNC_INTERVAL_SECONDS"), out var interval) ? interval : 60
        };
    }

    private static string GetOrCreateDeviceId()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GarageFlow");
        Directory.CreateDirectory(appDataPath);

        var idFile = Path.Combine(appDataPath, "device-id");
        if (File.Exists(idFile))
            return File.ReadAllText(idFile).Trim();

        var newId = Guid.NewGuid().ToString();
        File.WriteAllText(idFile, newId);
        return newId;
    }
}
