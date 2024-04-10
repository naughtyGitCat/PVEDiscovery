// // psyduck 2024-4-9
using Newtonsoft.Json;
namespace PVEDiscovery.Models;

public record IPAddress
{
    [JsonProperty("ip-address")]
    public string Address { get; set; }
    // 8, 128....
    [JsonProperty("prefix")]
    public int PrefixLength { get; set; }
    // ipv4 ipv6
    [JsonProperty("ip-address-type")]
    public string IPVersion { get; set; }
}

public record QEMUNetworkInterface
{
    public IEnumerable<IPAddress> IPAddress { get; set; }
    public string Name { get; set; }
    public string HardwareAddress { get; set; }
    public object Statistics { get; set; }
}

public record QEMUHostname
{
    [JsonProperty("host-name")]
    public string Hostname { get; set; }
}