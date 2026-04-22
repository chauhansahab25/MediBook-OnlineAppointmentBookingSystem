using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScheduleService.Entities;

public class AvailabilitySlot
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SlotId { get; set; }

    [Required]
    public int ProviderId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    public int DurationMinutes { get; set; }

    public bool IsBooked { get; set; } = false;

    public bool IsBlocked { get; set; } = false;

    // None, Daily, Weekly
    [MaxLength(50)]
    public string Recurrence { get; set; } = "None";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}