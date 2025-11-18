using clinic_management.infrastructure.Models;

public interface IPaymentStatusRepository : IRepository<PaymentStatus>
{

}
public class PaymentStatusRepository : Repository<PaymentStatus>, IPaymentStatusRepository
{
    public PaymentStatusRepository(ClinicManagementContext context) : base(context)
    {
    }
}