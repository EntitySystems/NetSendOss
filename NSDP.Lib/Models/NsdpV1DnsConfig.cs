using System.Text.Json.Serialization;

namespace NSDP.Lib.Models;

public class NsdpV1DnsConfig
{
    [JsonPropertyName("protocol")] public string Protocol { get; set; } = "NSDP_1";
    [JsonPropertyName("hosts")] public IReadOnlyList<NsdpV1DnsConfigHost> Hosts { get; set; } = [];
    [JsonPropertyName("sender_keys")] public IReadOnlyList<NsdpV1DnsConfigSenderKey> SenderKeys { get; set; } = [];
}

public class NsdpV1DnsConfigSenderKey
{
    [JsonPropertyName("alg")] public required string Algorithm { get; set; }
    [JsonPropertyName("pub_key")] public required string Key { get; set; }
}

public class NsdpV1DnsConfigHost
{
    [JsonPropertyName("cidr")] public required string? Cidr { get; set; }
    [JsonPropertyName("cname")] public required string? CName { get; set; }
    [JsonPropertyName("port")] public required int Port { get; set; }
    [JsonPropertyName("mode")] public required string Mode { get; set; }
}