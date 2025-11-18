using clinic_management.infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public interface IAppointmentStatusHistoryRepository : IRepository<AppointmentStatusHistory>
{
    public Task<List<AppointmentStatusHistory>> GetAppointmentStatusHistories(int appointmentId);


}

public class AppointmentStatusHistoryRepository : Repository<AppointmentStatusHistory>, IAppointmentStatusHistoryRepository
{
    public AppointmentStatusHistoryRepository(ClinicManagementContext context) : base(context)
    {
    }

    public async Task<List<AppointmentStatusHistory>> GetAppointmentStatusHistories(int appointmentId)
    {
        var param = new SqlParameter("@appointmentId", appointmentId);

        var result = await _context.AppointmentStatusHistories
            .FromSqlRaw("EXEC GetStatisticalAppointment @appointmentId", param)
            .ToListAsync();

        return result;
    }
}