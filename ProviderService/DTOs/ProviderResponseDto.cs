namespace ProviderService.DTOs;

public class ProviderResponseDto
{
    public int ProviderId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public string? Bio { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public string ClinicAddress { get; set; } = string.Empty;
    public double AvgRating { get; set; }
    public bool IsVerified { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
}
