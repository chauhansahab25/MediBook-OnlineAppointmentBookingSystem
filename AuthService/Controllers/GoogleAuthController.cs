using AuthService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Controllers;

[ApiController]
[Route("api/v1/auth/google")]
public class GoogleAuthController : ControllerBase
{
    private readonly IGoogleAuthService _googleAuthService;

    public GoogleAuthController(IGoogleAuthService googleAuthService)
    {
        _googleAuthService = googleAuthService;
    }

    /// <summary>Redirect to Google login page</summary>
    [HttpGet("login")]
    public IActionResult Login()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("Callback")
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>Google OAuth2 callback — handles token generation after Google login</summary>
    [HttpGet("callback")]
    public async Task<IActionResult> Callback()
    {
        // Read the Google authentication result
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Google authentication failed." });
        }

        // Extract user info from Google claims
        string? email = result.Principal?.FindFirstValue(ClaimTypes.Email);
        string? fullName = result.Principal?.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrEmpty(email))
        {
            return BadRequest(new { message = "Could not retrieve email from Google." });
        }

        // Handle login or auto-registration
        var response = await _googleAuthService.HandleGoogleLogin(
            email,
            fullName ?? email
        );

        return Ok(response);
    }
}