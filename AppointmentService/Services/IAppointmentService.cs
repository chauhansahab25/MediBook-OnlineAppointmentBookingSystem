using AppointmentService.DTOs;
using AppointmentService.Entities;

namespace AppointmentService.Services;

public interface IAppointmentService
{
    Task<AppointmentResponseDto> BookAppointment(BookAppointmentDto dto);

    Task<AppointmentResponseDto?> GetById(int appointmentId);

    Task<List<AppointmentResponseDto>> GetByPatient(int patientId);

    Task<List<AppointmentResponseDto>> GetByProvider(int providerId);

    Task<List<AppointmentResponseDto>> GetByProviderAndDate(int providerId, DateTime date);

    Task<bool> CancelAppointment(int appointmentId);

    Task<AppointmentResponseDto> RescheduleAppointment(int appointmentId, RescheduleAppointmentDto dto);

    Task<bool> CompleteAppointment(int appointmentId);

    Task<bool> UpdateStatus(int appointmentId, string status);

    Task<List<AppointmentResponseDto>> GetUpcomingByPatient(int patientId);

    Task<int> GetAppointmentCount(int providerId);
} 
