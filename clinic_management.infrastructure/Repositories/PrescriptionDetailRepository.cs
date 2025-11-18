using clinic_management.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IPrescriptionDetailRepository : IRepository<PrescriptionDetail>
{
    public Task<PrescriptionDetail?> GetPrescriptionDetailById(int presDetailId);
    public Task<List<PrescriptionDetail>> GetListPrescriptionDetailByPresId(Guid? currentUserId, string currentRoleName, string roleGuest, string roleDoctor, int presId);

}
public class PrescriptionDetailRepository : Repository<PrescriptionDetail>, IPrescriptionDetailRepository
{
    public PrescriptionDetailRepository(ClinicManagementContext context) : base(context)
    {
    }

    public async Task<PrescriptionDetail?> GetPrescriptionDetailById(int presDetailId)
    {
        var prescriptionDetail = await _dbSet.SingleOrDefaultAsync(pd => pd.PrescriptionDetailId == presDetailId);
        return prescriptionDetail;
    }
    public async Task<List<PrescriptionDetail>> GetListPrescriptionDetailByPresId(Guid? currentUserId, string currentRoleName, string roleGuest, string roleDoctor, int presId)
    {
        var query = _dbSet.Include(p => p.Medicine)
            .Include(pd => pd.Prescription).ThenInclude(p => p!.MedicalRecordDetail).ThenInclude(mrd => mrd!.Appointment)
            .Where(pd => pd.PrescriptionId == presId).AsQueryable();
        if (currentRoleName == roleGuest)
        {
            query = query.Where(a => a.Prescription!.MedicalRecordDetail!.Appointment!.PatientId == currentUserId);
        }
        else if (currentRoleName == roleDoctor)
        {
            query = query.Where(a => a.Prescription!.MedicalRecordDetail!.Appointment!.DoctorId == currentUserId);
        }
        query = query.Where(pd => pd.PrescriptionId == presId);
        var result = await query.ToListAsync();
        return result;
    }
}