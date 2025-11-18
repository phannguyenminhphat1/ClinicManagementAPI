using clinic_management.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IBillingDetailRepository : IRepository<BillingDetail>
{
    public Task<List<BillingDetail>> GetUnpaidBillingDetailsByBillingId(int billingId, int paymentStatusIdUnpaid);
}
public class BillingDetailRepository : Repository<BillingDetail>, IBillingDetailRepository
{
    public BillingDetailRepository(ClinicManagementContext context) : base(context)
    {
    }

    public async Task<List<BillingDetail>> GetUnpaidBillingDetailsByBillingId(int billingId, int paymentStatusIdUnpaid)
    {
        var lstBillingDetailUnpaid = await _dbSet.Where(b => b.BillingId == billingId && b.PaymentStatusId == paymentStatusIdUnpaid).ToListAsync();
        return lstBillingDetailUnpaid;
    }
}