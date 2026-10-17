using Microsoft.AspNetCore.Mvc;
using WristMatch.Application;

namespace WristMatch.Api.Controllers;

[ApiController]
[Route("api/v1/sessions")]
public sealed class SessionsController(IGuestSessionService sessions, IConfiguration configuration, IWebHostEnvironment environment) : ControllerBase
{
    public const string CookieName = "wristmatch.session";

    [HttpPost]
    public async Task<ActionResult<GuestSessionResult>> Create(CancellationToken cancellationToken)
    {
        var rawToken = Request.Cookies[CookieName];
        var result = await sessions.GetOrCreateAsync(rawToken, cancellationToken);
        var secure = configuration.GetValue("Security:CookieSecure", !environment.IsDevelopment());
        Response.Cookies.Append(CookieName, result.RawToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Expires = result.Session.ExpiresAt
        });
        return Ok(new GuestSessionResult(result.Session.Id, result.Session.ExpiresAt, result.Created));
    }
}
