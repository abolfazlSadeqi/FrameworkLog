using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace WebApiClient.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ValuesController : ControllerBase
{
    [HttpGet]
    public IActionResult Test()
    {

        Log.Information("test|username:fdfdfd|Password:123456dfdf");
        Log.Error("test2");
        Log.Error("test2");

        Log.Debug("test");

        return Ok();
    }
}
