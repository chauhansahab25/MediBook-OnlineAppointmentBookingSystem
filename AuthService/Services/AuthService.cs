 
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.DTOs;
using AuthService.Entities;
using AuthService.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    private static readonly Dictionary<string, int> RefreshTokenStore = new();

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> Register(RegisterDto dto)
    {
        bool emailTaken = await _userRepository.ExistsByEmail(dto.Email);

        if (emailTaken)
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Phone = dto.Phone,
            Role = dto.Role,
            Provider = "Local",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddUser(user);

        string token = GenerateJwtToken(user);
        string refreshToken = GenerateRefreshToken();
        RefreshTokenStore[refreshToken] = user.UserId;

        return BuildAuthResponse(user, token, refreshToken);
    }

    public async Task<AuthResponseDto> Login(LoginDto dto)
    {
        var user = await _userRepository.FindByEmail(dto.Email);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        bool passwordOk = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

        if (!passwordOk)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        string token = GenerateJwtToken(user);
        string refreshToken = GenerateRefreshToken();
        RefreshTokenStore[refreshToken] = user.UserId;

        return BuildAuthResponse(user, token, refreshToken);
    }

    public Task<bool> Logout(string refreshToken)
    {
        bool removed = RefreshTokenStore.Remove(refreshToken);
        return Task.FromResult(removed);
    }

    public Task<bool> ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(GetJwtKey());

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<AuthResponseDto> RefreshToken(string refreshToken)
    {
        if (!RefreshTokenStore.TryGetValue(refreshToken, out int userId))
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var user = await _userRepository.FindByUserId(userId);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User not found or inactive.");
        }

        RefreshTokenStore.Remove(refreshToken);

        string newToken = GenerateJwtToken(user);
        string newRefreshToken = GenerateRefreshToken();
        RefreshTokenStore[newRefreshToken] = user.UserId;

        return BuildAuthResponse(user, newToken, newRefreshToken);
    }

    public async Task<UserProfileDto> GetUserByEmail(string email)
    {
        var user = await _userRepository.FindByEmail(email);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        return MapToProfileDto(user);
    }

    public async Task<UserProfileDto> GetUserById(int userId)
    {
        var user = await _userRepository.FindByUserId(userId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        return MapToProfileDto(user);
    }

    public async Task<UserProfileDto> UpdateProfile(int userId, UpdateProfileDto dto)
    {
        var user = await _userRepository.FindByUserId(userId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (!string.IsNullOrWhiteSpace(dto.FullName))
        {
            user.FullName = dto.FullName;
        }

        if (!string.IsNullOrWhiteSpace(dto.Phone))
        {
            user.Phone = dto.Phone;
        }

        if (!string.IsNullOrWhiteSpace(dto.ProfilePicUrl))
        {
            user.ProfilePicUrl = dto.ProfilePicUrl;
        }

        await _userRepository.UpdateUser(user);

        return MapToProfileDto(user);
    }

    public async Task<bool> ChangePassword(int userId, ChangePasswordDto dto)
    {
        var user = await _userRepository.FindByUserId(userId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        bool currentPasswordOk = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);

        if (!currentPasswordOk)
        {
            throw new UnauthorizedAccessException("Current password is incorrect.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        await _userRepository.UpdateUser(user);

        return true;
    }

    public async Task<bool> DeactivateAccount(int userId)
    {
        var user = await _userRepository.FindByUserId(userId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        user.IsActive = false;

        await _userRepository.UpdateUser(user);

        return true;
    }

    // ── Private Helpers ──────────────────────────────────────────────────────

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtKey()));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        int expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    private string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private string GetJwtKey()
    {
        string? key = _configuration["Jwt:Key"];

        if (string.IsNullOrEmpty(key))
        {
            throw new InvalidOperationException("JWT key is not configured.");
        }

        return key;
    }

    private AuthResponseDto BuildAuthResponse(User user, string token, string refreshToken)
    {
        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role
        };
    }

    private UserProfileDto MapToProfileDto(User user)
    {
        return new UserProfileDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            Provider = user.Provider,
            ProfilePicUrl = user.ProfilePicUrl,
            CreatedAt = user.CreatedAt
        };
    }
}