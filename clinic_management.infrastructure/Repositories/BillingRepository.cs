using clinic_management.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IBillingRepository : IRepository<Billing>
{
    public Task<(List<Billing> AllBillings, int TotalRecords)> GetAllBillings(int page,
        int pageSize, DateTime? dateFilter = null, int? status = null, string? keyword = null);
    public Task<Billing?> GetBilling(int billingId);
    public Task<Billing?> GetBillingByAppointmentId(int appointmentId);

}
public class BillingRepository : Repository<Billing>, IBillingRepository
{
    public BillingRepository(ClinicManagementContext context) : base(context)
    {
    }

    public async Task<(List<Billing> AllBillings, int TotalRecords)> GetAllBillings(int page,
        int pageSize, DateTime? dateFilter = null, int? statusId = null, string? keyword = null)
    {
        var query = _dbSet
            .Include(b => b.Appointment).ThenInclude(a => a!.Patient)
            .Include(b => b.Appointment).ThenInclude(a => a!.Doctor).ThenInclude(a => a!.Specialty)
            .Include(b => b.Appointment).ThenInclude(a => a!.Status)
            .Include(b => b.BillingDetails).ThenInclude(bid => bid.PaymentStatus)
            .Include(b => b.BillingDetails).ThenInclude(bid => bid.Service)
            .Include(b => b.BillingMedicines).ThenInclude(bid => bid.PaymentStatus)
            .Include(b => b.BillingMedicines).ThenInclude(bid => bid.Medicine)
            .AsQueryable();

        if (dateFilter.HasValue)
        {
            var selectedDate = dateFilter.Value.Date; // chỉ lấy phần ngày
            var nextDate = selectedDate.AddDays(1);

            query = query.Where(a => a.CreatedAt >= selectedDate && a.CreatedAt < nextDate);
        }
        if (statusId.HasValue)
        {

            query = query.Where(b => b.BillingDetails.All(bd => bd.PaymentStatusId == statusId));

        }
        // if (status != null)
        // {
        //     query = query.Where(a => a.PaymentStatusId == status);
        // }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.Trim().ToLower();
            query = query.Where(a =>
                a.Appointment!.Patient!.Fullname!.ToLower().Contains(lowerKeyword) ||
                a.Appointment!.Patient.Phone!.Contains(lowerKeyword));
        }

        var totalRecords = await query.CountAsync();

        var billings = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (billings, totalRecords);
    }

    public async Task<Billing?> GetBilling(int billingId)
    {
        var billing = await _dbSet
            .Include(b => b.Appointment).ThenInclude(a => a!.Patient)
            .Include(b => b.Appointment).ThenInclude(a => a!.Doctor).ThenInclude(a => a!.Specialty)
            .Include(b => b.Appointment).ThenInclude(a => a!.Status)
            .Include(b => b.BillingDetails).ThenInclude(bid => bid.PaymentStatus)
            .Include(b => b.BillingDetails).ThenInclude(bid => bid.Service)
            .Include(b => b.BillingMedicines).ThenInclude(bid => bid.PaymentStatus)
            .Include(b => b.BillingMedicines).ThenInclude(bid => bid.Medicine)
            .FirstOrDefaultAsync(a => a.BillingId == billingId);
        return billing;
    }
    public async Task<Billing?> GetBillingByAppointmentId(int appointmentId)
    {
        var billing = await _dbSet
            .Include(b => b.Appointment).ThenInclude(a => a!.Patient)
            .Include(b => b.Appointment).ThenInclude(a => a!.Doctor).ThenInclude(a => a!.Specialty)
            .Include(b => b.Appointment).ThenInclude(a => a!.Status)
            .Include(b => b.BillingDetails).ThenInclude(bid => bid.PaymentStatus)
            .Include(b => b.BillingDetails).ThenInclude(bid => bid.Service)
            .Include(b => b.BillingMedicines).ThenInclude(bid => bid.PaymentStatus)
            .Include(b => b.BillingMedicines).ThenInclude(bid => bid.Medicine)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        return billing;
    }
}