using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Context;

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

    [HttpGet("log-control")]
    public IActionResult TestLogControl()
    {
        // 🔹 Tag "Payment" - Only Warning+ should log
        using (LogContext.PushProperty("Tag", "Payment"))
        {
            Log.Information("Payment info - should NOT appear");
            Log.Warning("Payment warning - should appear");
        }

        // 🔹 Tag "Auth" - Only Error+ should log
        using (LogContext.PushProperty("Tag", "Auth"))
        {
            Log.Warning("Auth warning - should NOT appear");
            Log.Error("Auth error - should appear");
        }

        // 🔹 Tag "VerboseAPI" - Disabled, should never log
        using (LogContext.PushProperty("Tag", "VerboseAPI"))
        {
            Log.Information("VerboseAPI info - should NOT appear");
        }

        Log.Information("Payment info - should NOT appear");
        Log.Error("Auth error - should appear");

        return Ok("Check console or log files for results.");
    }


}
