using System.Linq.Expressions;
using clinic_management.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IMedicalRecordDetailRepository : IRepository<MedicalRecordDetail>
{
    public Task<MedicalRecordDetail?> GetMedicalRecordDetailWithAppointmentAsync(Expression<Func<MedicalRecordDetail, bool>> predicate);
    public Task<(List<MedicalRecordDetail> MedicalRecordsDetail, int TotalRecords)> GetAllMedicalRecordDetail(Expression<Func<MedicalRecordDetail, bool>> predicate, Guid currentUserId, string currentRoleName, string roleGuest, string roleDoctor, int page, int pageSize, int completedStatus);
    public Task<MedicalRecordDetail?> GetMedicalRecordDetail(Expression<Func<MedicalRecordDetail, bool>> predicate, Guid currentUserId, string currentRoleName, string roleGuest, string roleDoctor);


}

public class MedicalRecordDetailRepository : Repository<MedicalRecordDetail>, IMedicalRecordDetailRepository
{
    public MedicalRecordDetailRepository(ClinicManagementContext context) : base(context)
    {
    }

    #region GET ALL MEDICAL RECORD DETAIL
    public async Task<(List<MedicalRecordDetail> MedicalRecordsDetail, int TotalRecords)> GetAllMedicalRecordDetail(Expression<Func<MedicalRecordDetail, bool>> predicate, Guid currentUserId, string currentRoleName, string roleGuest, string roleDoctor, int page, int pageSize, int completedStatus)
    {
        var query = _dbSet
            .Include(m => m.Appointment).ThenInclude(a => a!.Patient)
            .Include(m => m.Appointment).ThenInclude(a => a!.Doctor)
            .Include(m => m.Appointment).ThenInclude(a => a!.Status)
            .AsQueryable();
        if (currentRoleName == roleGuest)
        {
            query = query.Where(q => q.Appointment!.PatientId == currentUserId && q.Appointment!.StatusId == completedStatus);
        }
        else if (currentRoleName == roleDoctor)
        {
            query = query.Where(q => q.Appointment!.StatusId == completedStatus);
        }
        query = query.Where(predicate).AsNoTracking();
        var totalRecords = await query.CountAsync();

        var medicalRecordDetails = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (medicalRecordDetails, totalRecords);
    }
    #endregion
    public async Task<MedicalRecordDetail?> GetMedicalRecordDetailWithAppointmentAsync(Expression<Func<MedicalRecordDetail, bool>> predicate)
    {
        var medicalRecordDetail = await _dbSet.Include(u => u.Appointment).SingleOrDefaultAsync(predicate);
        return medicalRecordDetail;
    }

    #region GET MEDICAL RECORD DETAIL BY ID
    public async Task<MedicalRecordDetail?> GetMedicalRecordDetail(Expression<Func<MedicalRecordDetail, bool>> predicate, Guid currentUserId, string currentRoleName, string roleGuest, string roleDoctor)
    {
        var query = _dbSet
            .Include(m => m.Appointment).ThenInclude(a => a!.Patient)
            .Include(m => m.Appointment).ThenInclude(a => a!.Doctor)
            .Include(m => m.MedicalTests).ThenInclude(mt => mt.Service)
            .Include(m => m.MedicalTests).ThenInclude(mt => mt.Status)
            .Include(m => m.MedicalTests).ThenInclude(mt => mt.MedicalTestResults).AsQueryable();
        if (currentRoleName == roleGuest)
        {
            query = query.Where(q => q.Appointment!.PatientId == currentUserId);
        }
        else if (currentRoleName == roleDoctor)
        {
            query = query.Where(q => q.Appointment!.DoctorId == currentUserId);
        }
        var medicalRecordDetail = await query.SingleOrDefaultAsync(predicate);
        return medicalRecordDetail;

    }
    #endregion

}