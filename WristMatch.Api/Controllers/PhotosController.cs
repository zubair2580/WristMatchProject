using Microsoft.AspNetCore.Mvc;
using WristMatch.Application;
using WristMatch.Domain;

namespace WristMatch.Api.Controllers;

[ApiController]
[Route("api/v1/photos")]
public sealed class PhotosController(IGuestSessionService sessions, IPhotoAssetService photos) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(8 * 1024 * 1024)]
    public async Task<ActionResult<PhotoAssetResult>> Upload([FromForm] IFormFile? file, [FromForm] bool consent, [FromForm] string? consentVersion, CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(cancellationToken);
        if (session is null)
            return Unauthorized();
        if (file is null)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Photo required", detail: "Choose a JPEG or PNG photo to continue.");

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await photos.UploadAsync(new PhotoUploadRequest(session.Id, stream, file.ContentType, file.Length, consent, consentVersion ?? string.Empty), cancellationToken);
            return Created(result.PreviewUrl, result);
        }
        catch (PhotoValidationException exception)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Photo cannot be uploaded", detail: exception.Message, extensions: new Dictionary<string, object?> { ["code"] = exception.Code });
        }
    }

    [HttpGet("{id:guid}/image")]
    public async Task<IActionResult> GetImage(Guid id, CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(cancellationToken);
        if (session is null)
            return Unauthorized();
        var result = await photos.OpenAsync(session.Id, id, cancellationToken);
        if (result is null)
            return NotFound();
        Response.Headers.CacheControl = "no-store";
        return File(result.Value.Content, result.Value.Asset.MimeType);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(cancellationToken);
        if (session is null)
            return Unauthorized();
        return await photos.DeleteAsync(session.Id, id, cancellationToken) ? NoContent() : NotFound();
    }

    private async Task<GuestSession?> GetSessionAsync(CancellationToken cancellationToken)
    {
        var token = Request.Cookies[SessionsController.CookieName];
        return string.IsNullOrWhiteSpace(token) ? null : await sessions.FindAsync(token, cancellationToken);
    }
}
