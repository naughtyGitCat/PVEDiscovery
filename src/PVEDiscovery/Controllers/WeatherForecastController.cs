using Microsoft.AspNetCore.Mvc;
using PVEDiscovery.Services;

namespace PVEDiscovery.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly VirtualMachineService _vmService;
    private readonly ILogger<WeatherForecastController> _logger;
    
    public WeatherForecastController(VirtualMachineService virtualMachineService, ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
        _vmService = virtualMachineService;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpGet("Test")]
    public async Task<object> TestAsync()
    {
        try
        {
            var x = await _vmService.GetNodesAsync();
            return x;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
