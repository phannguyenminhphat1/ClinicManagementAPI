using System.Linq.Expressions;
using clinic_management.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IMedicalTestRepository : IRepository<MedicalTest>
{
    public Task<(List<MedicalTest> MedicalTests, int TotalRecords)> GetAllMedicalTests(Expression<Func<MedicalTest, bool>>? predicate, int page, int pageSize, DateTime? dateFilter = null, int? statusId = null, string? keyword = null);

    public Task<MedicalTest?> GetMedicalTestById(int medicalTestId);

    public Task<List<MedicalTest>> GetAllMedicalTestsByMedicalRecordDetailId(int MedicalRecordDetailId);
}
public class MedicalTestRepository : Repository<MedicalTest>, IMedicalTestRepository
{
    public MedicalTestRepository(ClinicManagementContext context) : base(context)
    {
    }

    public async Task<(List<MedicalTest> MedicalTests, int TotalRecords)> GetAllMedicalTests(Expression<Func<MedicalTest, bool>>? predicate, int page, int pageSize, DateTime? dateFilter = null, int? statusId = null, string? keyword = null)
    {
        var query = _dbSet
            .Include(mt => mt.Service)
            .Include(mt => mt.MedicalRecordDetail).ThenInclude(md => md!.Appointment).ThenInclude(a => a!.Patient)
            .Include(mt => mt.MedicalTestResults)
            .Include(mt => mt.Status)
            .AsQueryable();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        if (statusId.HasValue)
        {
            query = query.Where(a => a.Status!.StatusId == statusId);

        }
        if (dateFilter.HasValue)
        {
            var startTime = dateFilter.Value.Date.AddHours(1);
            var endTime = dateFilter.Value.Date.AddHours(24);
            query = query.Where(a => a.CreatedAt >= startTime && a.CreatedAt <= endTime);

        }
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.Trim().ToLower();
            query = query.Where(a =>
                a.Service!.ServiceName!.ToLower().Contains(lowerKeyword) || a.MedicalRecordDetail!.Appointment!.Patient!.Fullname!.ToLower().Contains(lowerKeyword));
        }

        var totalRecords = await query.CountAsync();

        var medicalTests = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (medicalTests, totalRecords);
    }

    public async Task<List<MedicalTest>> GetAllMedicalTestsByMedicalRecordDetailId(int medicalRecordDetailId)
    {
        var result = await _dbSet.Include(m => m.MedicalRecordDetail).Where(mt => mt.MedicalRecordDetailId == medicalRecordDetailId).ToListAsync();
        return result;
    }

    public async Task<MedicalTest?> GetMedicalTestById(int medicalTestId)
    {
        var result = await _dbSet.Include(m => m.MedicalRecordDetail).SingleOrDefaultAsync(mt => mt.MedicalTestId == medicalTestId);
        return result;
    }



}