using Microsoft.AspNetCore.Mvc;

namespace Todo.Core.Common;

/// <summary>
/// Abstract base controller with common behavior: standard response patterns and Idempotency-Key support.
/// Auth/user context stubbed for a future work order.
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Gets the Idempotency-Key header value if present (Backend blueprint). No Redis/caching in this WO.
    /// </summary>
    protected string? IdempotencyKey =>
        Request.Headers.TryGetValue("Idempotency-Key", out var value) ? value.ToString() : null;

    /// <summary>
    /// Returns a consistent error response shape per Backend blueprint: errors[] with code, message, field.
    /// </summary>
    protected IActionResult Error(string code, string message, string? field = null)
    {
        var error = new { code, message, field };
        return BadRequest(new { errors = new[] { error } });
    }

    /// <summary>
    /// Returns 404 with blueprint-style error body.
    /// </summary>
    protected IActionResult NotFoundError(string code, string message)
    {
        var error = new { code, message, field = (string?)null };
        return NotFound(new { errors = new[] { error } });
    }
}
