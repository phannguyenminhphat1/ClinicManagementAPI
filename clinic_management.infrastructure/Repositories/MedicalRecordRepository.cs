using System.Linq.Expressions;
using clinic_management.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IMedicalRecordRepository : IRepository<MedicalRecord>
{

    public Task<MedicalRecord?> GetMedicalRecordByPatient(Guid patientId);
    public Task<MedicalRecord?> GetMedicalRecordById(int medicalRecordId, Guid currentUserId, string currentRoleName, string roleGuest, string roleDoctor);


}

public class MedicalRecordRepository : Repository<MedicalRecord>, IMedicalRecordRepository
{
    public MedicalRecordRepository(ClinicManagementContext context) : base(context)
    {
    }

    public async Task<MedicalRecord?> GetMedicalRecordById(int medicalRecordId, Guid currentUserId, string currentRoleName, string roleGuest, string roleDoctor)
    {
        var query = _dbSet
            .Include(r => r.Patient)
            .Include(r => r.MedicalRecordDetails).ThenInclude(mrd => mrd.Appointment)
            .AsQueryable();
        if (currentRoleName == roleGuest)
        {
            query = query.Where(q => q.PatientId == currentUserId);
        }
        var result = await query.SingleOrDefaultAsync(r => r.MedicalRecordId == medicalRecordId);
        return result;
    }

    public async Task<MedicalRecord?> GetMedicalRecordByPatient(Guid patientId)
    {
        var record = await _dbSet.SingleOrDefaultAsync(r => r.PatientId == patientId);
        return record;
    }

}