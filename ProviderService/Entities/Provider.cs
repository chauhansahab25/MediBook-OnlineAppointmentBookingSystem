namespace ProviderService.Entities;

public class Provider
{
    public int ProviderId { get; set; }

    public int UserId { get; set; }

    public string Specialization { get; set; } = string.Empty;

    public string Qualification { get; set; } = string.Empty;

    public int ExperienceYears { get; set; }

    public string? Bio { get; set; }

    public string ClinicName { get; set; } = string.Empty;

    public string ClinicAddress { get; set; } = string.Empty;

    public double AvgRating { get; set; } = 0;

    public bool IsVerified { get; set; } = false;

    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}