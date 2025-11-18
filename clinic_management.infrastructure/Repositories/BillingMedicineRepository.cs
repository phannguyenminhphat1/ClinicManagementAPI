using clinic_management.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IBillingMedicineRepository : IRepository<BillingMedicine>
{
    public Task<List<BillingMedicine>> GetUnpaidBillingMedicineByBillingId(int billingId, int paymentStatusIdUnpaid);
}
public class BillingMedicineRepository : Repository<BillingMedicine>, IBillingMedicineRepository
{
    public BillingMedicineRepository(ClinicManagementContext context) : base(context)
    {
    }

    public async Task<List<BillingMedicine>> GetUnpaidBillingMedicineByBillingId(int billingId, int paymentStatusIdUnpaid)
    {
        var lstBillingMedicineUnpaid = await _dbSet.Where(b => b.BillingId == billingId && b.PaymentStatusId == paymentStatusIdUnpaid).ToListAsync();
        return lstBillingMedicineUnpaid;
    }
}