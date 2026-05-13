using AppointmentService.DTOs;
using AppointmentService.Entities;

namespace AppointmentService.Services;

public interface IAppointmentService
{
    Task<List<AppointmentResponseDto>> GetAll();

    Task<AppointmentResponseDto> BookAppointment(BookAppointmentDto dto);

    Task<AppointmentResponseDto?> GetById(int appointmentId);
    Task<AppointmentResponseDto?> GetBySlotId(int slotId);

    Task<List<AppointmentResponseDto>> GetByPatient(int patientId);

    Task<List<AppointmentResponseDto>> GetByProvider(int providerId);

    Task<List<AppointmentResponseDto>> GetByProviderAndDate(int providerId, DateTime date);

    Task<bool> CancelAppointment(int appointmentId, string cancelledBy = "Patient");

    Task<AppointmentResponseDto> RescheduleAppointment(int appointmentId, RescheduleAppointmentDto dto);

    Task<bool> CompleteAppointment(int appointmentId);

    Task<bool> UpdateStatus(int appointmentId, string status);

    Task<List<AppointmentResponseDto>> GetUpcomingByPatient(int patientId);

    Task<int> GetAppointmentCount(int providerId);
    Task<bool> DeleteAppointment(int appointmentId);
} 
