using System.Linq.Expressions;
using clinic_management.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IMedicalRecordSummaryRepository : IRepository<MedicalRecordSummary>
{
    public Task<(List<MedicalRecordSummary> medicalRecordSummaries, int TotalRecords)> GetAllMedicalRecordDetail(Expression<Func<MedicalRecordSummary, bool>> predicate, int page, int pageSize);

}

public class MedicalRecordSummaryRepository : Repository<MedicalRecordSummary>, IMedicalRecordSummaryRepository
{
    public MedicalRecordSummaryRepository(ClinicManagementContext context) : base(context)
    {
    }

    public async Task<(List<MedicalRecordSummary> medicalRecordSummaries, int TotalRecords)> GetAllMedicalRecordDetail(Expression<Func<MedicalRecordSummary, bool>> predicate, int page, int pageSize)
    {
        var query = _dbSet.Where(predicate);

        var totalRecords = await query.CountAsync();

        var medicalRecordSummaries = await query
            .OrderByDescending(a => a.ScheduledDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (medicalRecordSummaries, totalRecords);
    }
}