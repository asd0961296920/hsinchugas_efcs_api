using hsinchugas_efcs_api.Model;
using hsinchugas_efcs_api.Service;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace hsinchugas_efcs_api.Controllers
{
    [ApiController]
    [Route("")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        [HttpGet("GetWeatherForecast")]
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

        [HttpPost("api/DIG")]
        public async Task<IActionResult> PostDIG([FromBody] JsonElement rawJson)
        {


            string docData = JsonSerializer.Serialize(rawJson);


            return Ok(EfcsService.GenerateDIG(docData));
        }









    }
}
