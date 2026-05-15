using AppointmentService.Entities;

namespace AppointmentService.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> FindById(int appointmentId);

    Task<List<Appointment>> FindByPatientId(int patientId);

    Task<List<Appointment>> FindByProviderId(int providerId);

    Task<Appointment?> FindBySlotId(int slotId);

    Task<List<Appointment>> FindByStatus(string status);

    Task<List<Appointment>> FindByProviderIdAndAppointmentDate(int providerId, DateTime date);

    Task<List<Appointment>> FindUpcomingByPatientId(int patientId);

    Task<int> CountByProviderId(int providerId);

    Task<Appointment> Add(Appointment appointment);

    Task<Appointment> Update(Appointment appointment);

    Task<bool> Delete(int appointmentId);
}
