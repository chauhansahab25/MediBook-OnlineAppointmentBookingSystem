using AuthService.DTOs;

namespace AuthService.Services;

public interface IGoogleAuthService
{
    Task<AuthResponseDto> HandleGoogleLogin(string email, string fullName);
}