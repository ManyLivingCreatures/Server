
using ServerCommon.Services.Test;
using Microsoft.AspNetCore.Mvc;

namespace ManagerServer.Controllers.Test;

[ApiController]
[Route("[controller]")]
public class TestController(
    ILogger<TestController> logger,
    ITestService testService
) : ControllerBase
{
    [HttpGet(Name = "GetTest")]
    public async Task<ActionResult<string>> Get()
    {
        logger.LogInformation("TestController.Get called");
        var result = await testService.GetTestAsync();
        return Ok("Hello World! " + result);
    }
}