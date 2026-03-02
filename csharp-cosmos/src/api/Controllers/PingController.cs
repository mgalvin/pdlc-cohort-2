using Microsoft.AspNetCore.Mvc;
using Todo.Core.Common;

namespace Todo.Api.Controllers;

/// <summary>
/// Minimal v1 controller to exercise the API versioning and BaseController pipeline.
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
public class PingController : BaseController
{
    [HttpGet]
    public IActionResult Get() => Ok("pong");
}
