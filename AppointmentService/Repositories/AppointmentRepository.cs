using AppointmentService.Data;
using AppointmentService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly AppDbContext _context;

    public AppointmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> FindById(int appointmentId)
    {
        return await _context.Appointments.FindAsync(appointmentId);
    }

    public async Task<List<Appointment>> FindByPatientId(int patientId)
    {
        return await _context.Appointments
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();
    }

    public async Task<List<Appointment>> FindByProviderId(int providerId)
    {
        return await _context.Appointments
            .Where(a => a.ProviderId == providerId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();
    }

    public async Task<Appointment?> FindBySlotId(int slotId)
    {
        return await _context.Appointments
            .FirstOrDefaultAsync(a => a.SlotId == slotId);
    }

    public async Task<List<Appointment>> FindByStatus(string status)
    {
        return await _context.Appointments
            .Where(a => a.Status == status)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();
    }

    public async Task<List<Appointment>> FindByProviderIdAndAppointmentDate(
        int providerId, DateTime date)
    {
        return await _context.Appointments
            .Where(a => a.ProviderId == providerId
                     && a.AppointmentDate.Date == date.Date)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<List<Appointment>> FindUpcomingByPatientId(int patientId)
    {
        return await _context.Appointments
            .Where(a => a.PatientId == patientId
                     && a.AppointmentDate.Date >= DateTime.UtcNow.Date
                     && a.Status == "Scheduled")
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();
    }

    public async Task<int> CountByProviderId(int providerId)
    {
        return await _context.Appointments
            .CountAsync(a => a.ProviderId == providerId);
    }

    public async Task<Appointment> Add(Appointment appointment)
    {
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task<Appointment> Update(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task<bool> Delete(int appointmentId)
    {
        var appointment = await _context.Appointments.FindAsync(appointmentId);

        if (appointment == null)
        {
            return false;
        }

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();
        return true;
    }
} 
