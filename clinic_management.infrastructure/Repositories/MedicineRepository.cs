using clinic_management.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IMedicineRepository : IRepository<Medicine>
{
    public Task<Medicine?> GetMedicineById(int medicineId);
    public Task<List<Medicine>> GetAllMedicines();


}
public class MedicineRepository : Repository<Medicine>, IMedicineRepository
{
    public MedicineRepository(ClinicManagementContext context) : base(context)
    {
    }

    public async Task<List<Medicine>> GetAllMedicines()
    {
        var medicines = await _dbSet.ToListAsync();
        return medicines;
    }

    public async Task<Medicine?> GetMedicineById(int medicineId)
    {
        var medicine = await _dbSet.FirstOrDefaultAsync(m => m.MedicineId == medicineId);
        return medicine;
    }
}