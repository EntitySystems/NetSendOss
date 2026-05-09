using System.Net.Http.Json;
using DnsClient;
using Microsoft.Extensions.Logging;
using NetSend.Shared.Types;
using NSDP.Lib.Models;
using NSDP.Lib.Static;

namespace NSDP.Lib.Types;

public class NsdpDnsClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public NsdpDnsClient(
        HttpClient httpClient,
        ILogger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<NsdpV1DnsConfig, string>> GetDnsConfigAsync(string senderHost)
    {
        try
        {
            var client = new LookupClient();

            var record = await client.QueryAsync(senderHost, QueryType.TXT);
            var txtRecords = record.Answers.TxtRecords();
            var nsdpConfigUrl = txtRecords
                .FirstOrDefault(x => x.Text.Any(g => g.StartsWith(ProtocolKeys.NSDP_CONF)))
                ?.ToString();
            if (nsdpConfigUrl is null)
                return new("NSDP Config URL not found");

            using var request = new HttpRequestMessage(HttpMethod.Get, nsdpConfigUrl);
            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new(
                    $"NSDP Config server returned an error: " +
                    $"{await response.Content.ReadAsStringAsync()}");

            var config = await response.Content.ReadFromJsonAsync<NsdpV1DnsConfig>();
            if (config is null)
                return new("Invalid NSDP config format");

            return new(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to fetch NSDP DNS config");
            return new("Unable to fetch NSDP DNS config");
        }
    }
}