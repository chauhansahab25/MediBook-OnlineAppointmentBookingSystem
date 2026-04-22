using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReviewService.Entities;

public class Review
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ReviewId { get; set; }

    // One review per appointment — enforced by unique index
    [Required]
    public int AppointmentId { get; set; }

    [Required]
    public int PatientId { get; set; }

    [Required]
    public int ProviderId { get; set; }

    // Rating must be between 1 and 5
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

    // Admin can verify a review
    public bool IsVerified { get; set; } = false;

    // Patient can choose to post anonymously
    public bool IsAnonymous { get; set; } = false;
}
