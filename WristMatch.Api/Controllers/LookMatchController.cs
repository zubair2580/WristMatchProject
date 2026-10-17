using Microsoft.AspNetCore.Mvc;
using WristMatch.Application;
using WristMatch.Domain;

namespace WristMatch.Api.Controllers;

[ApiController]
[Route("api/v1/look-match")]
public sealed class LookMatchController(IGuestSessionService sessions, ILookMatchService lookMatch) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(8 * 1024 * 1024)]
    public async Task<ActionResult<LookMatchResult>> Analyze(
        [FromForm] IFormFile? file,
        [FromForm] bool consent,
        [FromForm] string? consentVersion,
        CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(cancellationToken);
        if (session is null)
            return Unauthorized();
        if (file is null)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Selfie required", detail: "Upload a clear face selfie for Look Match.");
        if (!consent)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Consent required", detail: "Confirm consent before running Skin Analysis.");

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await lookMatch.AnalyzeAsync(session.Id, stream, file.ContentType, file.Length, consent, consentVersion ?? "2026-10-11", cancellationToken);
            return Ok(result);
        }
        catch (PhotoValidationException exception)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Photo cannot be analysed", detail: exception.Message, extensions: new Dictionary<string, object?> { ["code"] = exception.Code });
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Problem(statusCode: StatusCodes.Status502BadGateway, title: "Look Match failed", detail: exception.Message);
        }
    }

    [HttpGet("status")]
    public ActionResult<object> Status([FromServices] IConfiguration configuration)
    {
        var configured = !string.IsNullOrWhiteSpace(configuration["YouCam:ApiKey"]);
        var preferYouCam = string.Equals(configuration["TryOn:ProviderMode"], "youcam", StringComparison.OrdinalIgnoreCase);
        return Ok(new
        {
            youCamConfigured = configured,
            tryOnProviderMode = configured && preferYouCam ? "youcam" : "demo",
        });
    }

    private async Task<GuestSession?> GetSessionAsync(CancellationToken cancellationToken)
    {
        var token = Request.Cookies[SessionsController.CookieName];
        return string.IsNullOrWhiteSpace(token) ? null : await sessions.FindAsync(token, cancellationToken);
    }
}
