using clinic_management.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IPrescriptionRepository : IRepository<Prescription>
{
    public Task<Prescription?> GetPrescriptionByMrdId(Guid? currentUserId, string currentRoleName, string roleGuest, string roleDoctor, int medicalRecordDetailId);
    public Task<Prescription?> GetPrescriptionById(int presId);

}
public class PrescriptionRepository : Repository<Prescription>, IPrescriptionRepository
{
    public PrescriptionRepository(ClinicManagementContext context) : base(context)
    {
    }

    public async Task<Prescription?> GetPrescriptionByMrdId(Guid? currentUserId, string currentRoleName, string roleGuest, string roleDoctor, int medicalRecordDetailId)
    {
        var query = _dbSet.Include(p => p.MedicalRecordDetail).ThenInclude(mrd => mrd!.Appointment).AsQueryable();
        if (currentRoleName == roleGuest)
        {
            query = query.Where(a => a.MedicalRecordDetail!.Appointment!.PatientId == currentUserId);
        }
        else if (currentRoleName == roleDoctor)
        {
            query = query.Where(a => a.MedicalRecordDetail!.Appointment!.DoctorId == currentUserId);
        }
        var result = await query.SingleOrDefaultAsync(p => p.MedicalRecordDetailId == medicalRecordDetailId);
        return result;
    }

    public async Task<Prescription?> GetPrescriptionById(int presId)
    {
        var result = await _dbSet.Include(p => p.PrescriptionDetails).Include(p => p.MedicalRecordDetail).SingleOrDefaultAsync(p => p.PrescriptionId == presId);
        return result;
    }
}