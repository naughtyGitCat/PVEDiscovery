// // 张锐志 2023-04-24
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PVEDiscovery.Common;
namespace PVEDiscovery.Services;

public record VMReqParams
{
    /// <summary>
    /// FillIPWithHostname,
    /// if fetch ip address failed, return hostname instead of ip
    /// this could be work in mdns environment
    /// default: true
    /// </summary>
    public bool FillIPWithHostname { get; set; } = true;
    /// <summary>
    /// Ignore vm ipv6 address
    /// default: false
    /// </summary>
    public bool IgnoreV6 { get; set; } = false;
    /// <summary>
    /// Prefer ipv6 address instead of ipv4, if ipv6 exists
    /// default: false
    /// </summary>
    public bool PreferV6 { get; set; } = false;
    /// <summary>
    /// Ignore vm ipv4 address
    /// default: false
    /// </summary>
    public bool IgnoreV4 { get; set; } = false;
    /// <summary>
    /// Prefer ipv4 address instead of ipv6, if ipv4 exists
    /// default: true
    /// </summary>
    public bool PreferV4 { get; set; } = true;
    /// <summary>
    /// Ignore not running vm hosts, avoid scrape error
    /// </summary>
    public bool IgnoreNotRunning { get; set; } = true;
}

public record VirtualMachineDTO
{
    [JsonProperty("maxdisk")]
    public long MaxDisk { get; set; }

    [JsonProperty("cpu")]
    public long CPU { get; set; }

    [JsonProperty("vmid")]
    public long VmID { get; set; }

    [JsonProperty("netout")]
    public long NetOut { get; set; }

    [JsonProperty("diskwrite")]
    public long DiskWrite { get; set; }

    [JsonProperty("diskread")]
    public long DiskRead { get; set; }

    [JsonProperty("disk")]
    public long Disk { get; set; }

    [JsonProperty("balloon_min")]
    public long BalloonMin { get; set; }

    [JsonProperty("maxmem")]
    public long MaxMem { get; set; }

    [JsonProperty("status")]
    public string? Status { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("cpus")]
    public long CPUs { get; set; }

    [JsonProperty("shares")]
    public long Shares { get; set; }

    [JsonProperty("uptime")]
    public long Uptime { get; set; }

    [JsonProperty("netin")]
    public long NetIn { get; set; }

    [JsonProperty("mem")]
    public long Mem { get; set; }
}

public record ServiceDiscoveryDTO
{
    [JsonProperty("targets")]
    public IEnumerable<string> Targets { get; set; } = Array.Empty<string>();
    [JsonProperty("labels")]
    public Dictionary<string, string> Labels { get; set; } = new ();
}

public interface IVirtualMachineService
{
    public Task<IEnumerable<ServiceDiscoveryDTO>> GetExportersAsync(VMReqParams reqParams);
}

public record PVEStdResp<T>
{
    public T? Data { get; set; }
}

public record NodeDTO
{
    [JsonProperty("maxcpu")]
    public long MaxCPU { get; set; }

    [JsonProperty("id")]
    public string? ID { get; set; }

    [JsonProperty("maxdisk")]
    public long MaxDisk { get; set; }

    [JsonProperty("level")]
    public string? Level { get; set; }

    [JsonProperty("cpu")]
    public double Cpu { get; set; }

    [JsonProperty("status")]
    public string? Status { get; set; }

    [JsonProperty("uptime")]
    public long Uptime { get; set; }

    [JsonProperty("disk")]
    public long Disk { get; set; }

    [JsonProperty("type")]
    public string? Type { get; set; }

    [JsonProperty("node")]
    public string? Node { get; set; }

    [JsonProperty("ssl_fingerprint")]
    public string? SslFingerprint { get; set; }

    [JsonProperty("mem")]
    public long Mem { get; set; }

    [JsonProperty("maxmem")]
    public long MaxMem { get; set; }
}

public class VirtualMachineService : IVirtualMachineService
{
    private readonly HttpClient _httpClient;
    private readonly PVESettings _pveSettings;
    private readonly ILogger<VirtualMachineService> _logger;
    public VirtualMachineService(ILogger<VirtualMachineService> logger, IHttpClientFactory httpClientFactory, IOptions<PVESettings> options)
    {
        _logger = logger;
        _pveSettings = options.Value;
        _httpClient = httpClientFactory.CreateClient();
    }

    private async Task<IEnumerable<NodeDTO>> GetNodesAsync()
    {
        var ret = new List<NodeDTO>();
        foreach (var node in _pveSettings.Clusters)
        {
            _httpClient.BaseAddress = new Uri($"https://{node.Url}");
            var result = await _httpClient.GetAsync("/api2/json/nodes");
            if (!result.IsSuccessStatusCode)
            {
                _logger.LogWarning("request failed");
                continue;
            }
            var c=await result.Content.ReadAsStringAsync();
            var dto = JsonConvert.DeserializeObject<PVEStdResp<IEnumerable<NodeDTO>>>(c);
            ret.AddRange(dto!.Data!);
        }
        return ret;
    }

    private Task<object> GetVirtualMachinesAsync(string nodeID)
    {
        _httpClient.BaseAddress = new Uri($"https://{node.Url}");
        var result = await _httpClient.GetAsync("/api2/json/nodes");
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<ServiceDiscoveryDTO>> GetExportersAsync(VMReqParams reqParams)
    {
        throw new NotImplementedException();
    }
}
