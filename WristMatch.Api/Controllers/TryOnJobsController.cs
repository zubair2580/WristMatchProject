using Microsoft.AspNetCore.Mvc;
using WristMatch.Application;
using WristMatch.Domain;

namespace WristMatch.Api.Controllers;

[ApiController]
[Route("api/v1/tryon-jobs")]
public sealed class TryOnJobsController(IGuestSessionService sessions, ITryOnJobService jobs) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TryOnJobResult>> Create([FromBody] CreateTryOnBody body, CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(cancellationToken);
        if (session is null)
            return Unauthorized();
        var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault() ?? body.IdempotencyKey;
        try
        {
            var result = await jobs.CreateAsync(session.Id, new TryOnCreateRequest(body.PhotoId, body.ProductId, idempotencyKey ?? string.Empty), cancellationToken);
            return Accepted(result.ResultUrl ?? $"/api/v1/tryon-jobs/{result.Id}", result);
        }
        catch (TryOnValidationException exception)
        {
            var status = exception.Code == "IDEMPOTENCY_KEY_CONFLICT" ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest;
            return Problem(statusCode: status, title: "Try-on request rejected", detail: exception.Message, extensions: new Dictionary<string, object?> { ["code"] = exception.Code });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TryOnJobResult>> Get(Guid id, CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(cancellationToken);
        if (session is null)
            return Unauthorized();
        var result = await jobs.GetAsync(session.Id, id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/result")]
    public async Task<IActionResult> GetResult(Guid id, CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(cancellationToken);
        if (session is null)
            return Unauthorized();
        var result = await jobs.OpenResultAsync(session.Id, id, cancellationToken);
        if (result is null)
            return NotFound();
        Response.Headers.CacheControl = "no-store";
        return File(result.Value.Content, "image/jpeg");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(cancellationToken);
        if (session is null)
            return Unauthorized();
        return await jobs.DeleteAsync(session.Id, id, cancellationToken) ? NoContent() : NotFound();
    }

    private async Task<GuestSession?> GetSessionAsync(CancellationToken cancellationToken)
    {
        var token = Request.Cookies[SessionsController.CookieName];
        return string.IsNullOrWhiteSpace(token) ? null : await sessions.FindAsync(token, cancellationToken);
    }
}

public sealed record CreateTryOnBody(Guid PhotoId, Guid ProductId, string? IdempotencyKey = null);
