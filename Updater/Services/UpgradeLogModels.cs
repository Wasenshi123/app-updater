using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Updater.Services
{
    public class UpgradeLog
    {
        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonPropertyName("upgradeId")]
        public string? UpgradeId { get; set; }

        [JsonPropertyName("upgradeName")]
        public string? UpgradeName { get; set; }

        [JsonPropertyName("fromVersion")]
        public string? FromVersion { get; set; }

        [JsonPropertyName("toVersion")]
        public string? ToVersion { get; set; }

        [JsonPropertyName("status")]
        public UpgradeStatus Status { get; set; }

        [JsonPropertyName("stage")]
        public UpgradeStage Stage { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("details")]
        public Dictionary<string, object>? Details { get; set; }
    }

    public enum UpgradeStatus
    {
        Started,
        InProgress,
        Completed,
        Failed,
        Skipped
    }

    public enum UpgradeStage
    {
        Check,
        Download,
        Extract,
        PreInstall,
        Install,
        PostInstall,
        Cleanup
    }

    public class UpgradeSession
    {
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("startTime")]
        public DateTimeOffset StartTime { get; set; }

        [JsonPropertyName("endTime")]
        public DateTimeOffset? EndTime { get; set; }

        [JsonPropertyName("fromVersion")]
        public string? FromVersion { get; set; }

        [JsonPropertyName("toVersion")]
        public string? ToVersion { get; set; }

        [JsonPropertyName("overallStatus")]
        public UpgradeStatus OverallStatus { get; set; }

        [JsonPropertyName("upgrades")]
        public List<UpgradeLog> Upgrades { get; set; } = new List<UpgradeLog>();

        [JsonPropertyName("packageSize")]
        public long? PackageSize { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
