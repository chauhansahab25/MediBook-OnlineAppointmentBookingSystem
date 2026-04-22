 
using AuthService.DTOs;

namespace AuthService.Services;

public interface IAuthService
{
    Task<AuthResponseDto> Register(RegisterDto registerDto);
    Task<AuthResponseDto> Login(LoginDto loginDto);
    Task<bool> Logout(string token);
    Task<bool> ValidateToken(string token);
    Task<AuthResponseDto> RefreshToken(string refreshToken);
    Task<UserProfileDto> GetUserByEmail(string email);
    Task<UserProfileDto> GetUserById(int userId);
    Task<UserProfileDto> UpdateProfile(int userId, UpdateProfileDto updateProfileDto);
    Task<bool> ChangePassword(int userId, ChangePasswordDto changePasswordDto);
    Task<bool> DeactivateAccount(int userId);
}