using hsinchugas_efcs_api.Model;
using hsinchugas_efcs_api.Service;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace hsinchugas_efcs_api.Controllers
{
    [ApiController]
    [Route("")]
    public class EfcsController : ControllerBase
    {
        [HttpPost("api/efcs/b207")]
        public async Task<IActionResult> PostB207([FromBody] ALL<BillerDataQueryRq> request)
        {
            var data = new
            {
                msg = "ok"
            };

            string docData = JsonSerializer.Serialize(data);







            return Ok(EfcsService.GenerateDIG(docData));
        }

        [HttpPost("api/efcs/b208")]
        public async Task<IActionResult> PostB208()
        {
            var data = new
            {
                msg = "ok"
            };
            return Ok(data);
        }

        [HttpPost("api/efcs/b219")]
        public async Task<IActionResult> PostB219()
        {
            var data = new
            {
                msg = "ok"
            };
            return Ok(data);
        }
    }
}
