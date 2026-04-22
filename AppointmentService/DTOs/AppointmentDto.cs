namespace AppointmentService.DTOs;

public class BookAppointmentDto
{
    public int PatientId { get; set; }
    public int ProviderId { get; set; }
    public int SlotId { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string ModeOfConsultation { get; set; } = "InPerson";
    public string? Notes { get; set; }
}

public class RescheduleAppointmentDto
{
    public int NewSlotId { get; set; }
    public DateTime NewAppointmentDate { get; set; }
    public TimeSpan NewStartTime { get; set; }
    public TimeSpan NewEndTime { get; set; }
}

public class UpdateStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class AppointmentResponseDto
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int ProviderId { get; set; }
    public int SlotId { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string ModeOfConsultation { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 
