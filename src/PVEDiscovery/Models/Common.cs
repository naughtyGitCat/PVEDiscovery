// // psyduck 2024-4-9
using Newtonsoft.Json;
namespace PVEDiscovery.Models;

public record ResultWrapper<T>
{
    [JsonProperty("result")]
    public T Result { get; set; }
}
