using clinic_management.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IMedicalTestResultRepository : IRepository<MedicalTestResult>
{
    public Task<List<MedicalTestResult>?> GetMedicalTestsResultById(int medicalTestId);
    public Task AddListMedicalTestResult(List<MedicalTestResult> medicalTestResults);
    public Task<MedicalTestResult?> GetMedicalTestResultByMedicalTestResultId(int medicalTestResultId);
}
public class MedicalTestResultRepository : Repository<MedicalTestResult>, IMedicalTestResultRepository
{
    public MedicalTestResultRepository(ClinicManagementContext context) : base(context)
    {
    }


    public async Task<List<MedicalTestResult>?> GetMedicalTestsResultById(int medicalTestId)
    {
        var result = await _dbSet.Where(x => x.MedicalTestId == medicalTestId).ToListAsync();
        return result;
    }

    public async Task<MedicalTestResult?> GetMedicalTestResultByMedicalTestResultId(int medicalTestResultId)
    {
        var result = await _dbSet.SingleOrDefaultAsync(x => x.MedicalTestResultId == medicalTestResultId);
        return result;
    }

    public async Task AddListMedicalTestResult(List<MedicalTestResult> medicalTestResults)
    {
        await _dbSet.AddRangeAsync(medicalTestResults);
    }


}