// // 张锐志 2023-04-24
namespace PVEDiscovery.Common;

public record PVEClusterAuth
{
    public string Url { get; set; } = "127.0.0.1:8006";
    public string AuthType { get; set; } = "PVEAPIToken";
    public string PVEAPIToken { get; set; } = "";
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
}

public record Prometheus
{
    // node exporter expose port in the qemu instance
    public int NodeExporterPort { get; set; }
    // expose/proxy qemu or other metric from pve component to prom compatible metrics
    public bool ExposeInternalMetric { get; set; }
}

public class PVESettings
{
    public Prometheus Prometheus { get; set; }
    public IEnumerable<PVEClusterAuth> Clusters { get; set; } = ArraySegment<PVEClusterAuth>.Empty;
}
