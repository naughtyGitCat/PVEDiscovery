// // 张锐志 2023-04-24
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PVEDiscovery.Common;
using PVEDiscovery.Models;
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
    public string ID { get; set; } = string.Empty;

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

public record QEMU
{
    [JsonProperty("status", Required = Required.Always)]
    public string? Status { get; set; }

    [JsonProperty("mem", Required = Required.Always)]
    public long Mem { get; set; }
    
    [JsonProperty("maxmem", Required = Required.Always)]
    public long MemMaxBytes { get; set; }

    [JsonProperty("diskread", Required = Required.Always)]
    public long DiskRead { get; set; }
    
    [JsonProperty("diskwrite", Required = Required.Always)]
    public long DiskWrite { get; set; }

    [JsonProperty("netin", Required = Required.Always)]
    public long NetIn { get; set; }
    
    [JsonProperty("netout", Required = Required.Always)]
    public long NetOut { get; set; }

    [JsonProperty("vmid", Required = Required.Always)]
    public int VmId { get; set; }

    [JsonProperty("disk", Required = Required.Always)]
    public long Disk { get; set; }

    [JsonProperty("maxdisk", Required = Required.Always)]
    public long MaxDiskSize { get; set; }

    [JsonProperty("cpu", Required = Required.Always)]
    public long CPU { get; set; }

    [JsonProperty("name", Required = Required.Always)]
    public string? Name { get; set; }

    [JsonProperty("shares", Required = Required.Always)]
    public long Shares { get; set; }

    [JsonProperty("uptime", Required = Required.Always)]
    public long Uptime { get; set; }

    [JsonProperty("balloon_min", Required = Required.Always)]
    public long MemoryBalloonMinBytes { get; set; }

    [JsonProperty("cpus", Required = Required.Always)]
    public long CPUCores { get; set; }
}

public record NodeExtDTO : NodeDTO
{
    
}

public class VirtualMachineService : IVirtualMachineService
{
    private readonly PVESettings _pveSettings;
    private readonly HttpClientHandler _insecureHandler;
    private readonly ILogger<VirtualMachineService> _logger;
    public VirtualMachineService(ILogger<VirtualMachineService> logger, IOptions<PVESettings> options)
    {
        _logger = logger;
        _pveSettings = options.Value;
        _insecureHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
    }

    public async Task<IEnumerable<NodeDTO>> GetNodesAsync()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        _logger.LogInformation("_pveSettings.Clusters: {s}", _pveSettings.Clusters);
        var ret = new List<NodeDTO>();
        foreach (var node in _pveSettings.Clusters)
        {
            using var httpClient = new HttpClient(handler);
            switch (node.AuthType)
            {
                case "Token": httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("PVEAPIToken",node.PVEAPIToken); break;
                default: _logger.LogInformation("not implemented"); break;
            }
            
            _logger.LogDebug("{node}", node);
            var result = await httpClient.GetAsync($"https://{node.Url}/api2/json/nodes");
            if (!result.IsSuccessStatusCode)
            {
                _logger.LogWarning("request failed, {c},{x}",result.StatusCode, await result.Content.ReadAsStringAsync());
                continue;
            }
            var c=await result.Content.ReadAsStringAsync();
            var dto = JsonConvert.DeserializeObject<PVEStdResp<IEnumerable<NodeDTO>>>(c);
            ret.AddRange(dto!.Data!);
        }
        return ret;
    }

    private async Task<IEnumerable<QEMU>> GetClusterNodeQEMUsAsync(PVEClusterAuth pveCluster, string pveNodeName)
    {
        using var httpClient = new HttpClient(_insecureHandler);
        switch (pveCluster.AuthType)
        {
            case "Token": httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("PVEAPIToken",pveCluster.PVEAPIToken); break;
            default: _logger.LogInformation("not implemented"); break;
        }
        var result = await httpClient.GetAsync($"https://{pveCluster.Url}/api2/json/nodes/{pveNodeName}/qemu");
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"request failed, {result.StatusCode},{await result.Content.ReadAsStringAsync()}");
        }
        var c=await result.Content.ReadAsStringAsync();
        var raw= JsonConvert.DeserializeObject<PVEStdResp<IEnumerable<QEMU>>>(c);
        return raw!.Data is null ? Array.Empty<QEMU>() : raw.Data;
    }
    
    private async Task<QEMUAgentInfo> GetQEMUAgentInfoAsync(PVEClusterAuth pveCluster, string pveNodeName, int qemuId)
    {
        using var httpClient = new HttpClient(_insecureHandler);
        switch (pveCluster.AuthType)
        {
            case "Token": httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("PVEAPIToken",pveCluster.PVEAPIToken); break;
            default: _logger.LogInformation("not implemented"); break;
        }
        var result = await httpClient.GetAsync($"https://{pveCluster.Url}/api2/json/nodes/{pveNodeName}/qemu/{qemuId}/agent/info");
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"request failed, {result.StatusCode},{await result.Content.ReadAsStringAsync()}");
        }
        var c = await result.Content.ReadAsStringAsync();
        var raw = JsonConvert.DeserializeObject<PVEStdResp<QEMUAgentInfoDTO>>(c);
        if (raw!.Data is null) throw new QEMUAgentNotEnabledException("agent not enabled");
        return raw.Data.Result;
    }
    
    private async Task<IEnumerable<QEMUNetworkInterface>> GetNetworkInterfacesAsync(PVEClusterAuth pveCluster,string pveNodeName, int qemuId)
    {
        var qemuAgent = await GetQEMUAgentInfoAsync(pveCluster, pveNodeName, qemuId);
        if (qemuAgent.SupportedCommands.All(c => c.Name != "network-get-interfaces")) throw new QEMUAgentNotSupportedException("agent not support this command");
        if (!qemuAgent.SupportedCommands.First(c=> c.Name=="network-get-interfaces").Enabled)  throw new QEMUAgentFeatureDisabledException("agent disabled this command");
        using var httpClient = new HttpClient(_insecureHandler);
        switch (pveCluster.AuthType)
        {
            case "Token": httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("PVEAPIToken",pveCluster.PVEAPIToken); break;
            default: _logger.LogInformation("not implemented"); break;
        }
        var result = await httpClient.GetAsync($"https://{pveCluster.Url}/api2/json/nodes/{pveNodeName}/qemu/{qemuId}/agent/network-get-interfaces");
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"request failed, {result.StatusCode},{await result.Content.ReadAsStringAsync()}");
        }
        var c = await result.Content.ReadAsStringAsync();
        var raw = JsonConvert.DeserializeObject<PVEStdResp<ResultWrapper<IEnumerable<QEMUNetworkInterface>>>>(c);
        return raw!.Data is null ? Array.Empty<QEMUNetworkInterface>() : raw.Data!.Result;
    }

    private async Task<string> GetHostnameAsync(PVEClusterAuth pveCluster, string pveNodeName, int qemuId)
    {
        var qemuAgent = await GetQEMUAgentInfoAsync(pveCluster, pveNodeName, qemuId);
        if (qemuAgent!.SupportedCommands.All(c => c.Name != "get-host-name")) throw new QEMUAgentNotSupportedException("agent not support this command");
        if (!qemuAgent.SupportedCommands.First(c=> c.Name=="get-host-name").Enabled)  throw new QEMUAgentFeatureDisabledException("agent disabled this command");
        using var httpClient = new HttpClient(_insecureHandler);
        switch (pveCluster.AuthType)
        {
            case "Token": httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("PVEAPIToken",pveCluster.PVEAPIToken); break;
            default: _logger.LogInformation("not implemented"); break;
        }
        var result = await httpClient.GetAsync($"https://{pveCluster.Url}/api2/json/nodes/{pveNodeName}/qemu/{qemuId}/agent/get-host-name");
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"request failed, {result.StatusCode},{await result.Content.ReadAsStringAsync()}");
        }
        var c = await result.Content.ReadAsStringAsync();
        var raw = JsonConvert.DeserializeObject<PVEStdResp<ResultWrapper<QEMUHostname>>>(c);
        return raw!.Data!.Result.Hostname;
    }

    public async Task<IEnumerable<ServiceDiscoveryDTO>> GetQEMUExportersAsync(PVEClusterAuth pveCluster)
    {
        var sds = new List<ServiceDiscoveryDTO>();
        var nodes = await GetNodesAsync();
        foreach (var node in nodes)
        {
            var nodeName = node.ID.Split("id/")[1];
            var qemus = await GetClusterNodeQEMUsAsync(pveCluster, nodeName);
            foreach (var qemu in qemus)
            {
                try
                {
                    var hostname = await GetHostnameAsync(pveCluster, nodeName, qemu.VmId);
                    var interfaces = await GetNetworkInterfacesAsync(pveCluster, nodeName, qemu.VmId);
                    sds.AddRange(from networkInterface in interfaces
                                 from address in networkInterface.IPAddress
                                 where address.IPVersion == "ipv4"
                                 select new ServiceDiscoveryDTO
                                 {
                                     Targets = new[]
                                     {
                                         $"{address.Address}:{_pveSettings.Prometheus.NodeExporterPort}"
                                     },
                                     Labels = new Dictionary<string, string>()
                                     {
                                         {
                                             "ip", address.Address
                                         },
                                         {
                                             "port", $"{_pveSettings.Prometheus.NodeExporterPort}"
                                         },
                                         {
                                             "hostname", hostname
                                         },
                                         {
                                             "id", qemu.VmId.ToString()
                                         }
                                     }
                                 });
                }
                catch (QEMUAgentException e)
                {
                    _logger.LogWarning("{qemu}, {e}",e,qemu);
                } 
            }
        }
        return sds;
    }

    public Task<IEnumerable<ServiceDiscoveryDTO>> GetExportersAsync(VMReqParams reqParams)
    {
        throw new NotImplementedException();
    }
}
