using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.DTOs;
using AuthService.Entities;
using AuthService.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    private static readonly Dictionary<string, int> RefreshTokenStore = new();

    public GoogleAuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> HandleGoogleLogin(string email, string fullName)
    {
        // Check if user already exists
        var user = await _userRepository.FindByEmail(email);

        // If not, register them automatically
        if (user == null)
        {
            user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = string.Empty,
                Role = "Patient",
                Provider = "Google",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddUser(user);
        }

        // Generate JWT token
        string token = GenerateJwtToken(user);
        string refreshToken = GenerateRefreshToken();

        RefreshTokenStore[refreshToken] = user.UserId;

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role
        };
    }

    // ── Private Helpers ──────────────────────────────────────────────────────

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(GetJwtKey()));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        int expiryMinutes = int.Parse(
            _configuration["Jwt:ExpiryMinutes"] ?? "60");

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
}