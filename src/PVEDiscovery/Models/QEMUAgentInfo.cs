// // psyduck 2024-4-9
using Newtonsoft.Json;
namespace PVEDiscovery.Models;

public record SupportedCommand
{
    [JsonProperty("enabled")]
    public bool Enabled { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    [JsonProperty("success-response")]
    public bool SuccessResponse { get; set; }
}

public record QEMUAgentInfo
{
    public string Version { get; set; } = string.Empty;
    public IEnumerable<SupportedCommand> SupportedCommands { get; set; } = Array.Empty<SupportedCommand>();
}

public record QEMUAgentInfoDTO
{
    [JsonProperty("result")]
    public QEMUAgentInfo Result { get; set; }
}
