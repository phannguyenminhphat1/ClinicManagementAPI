using clinic_management.infrastructure.Models;

public interface IPaymentRepository : IRepository<Payment>
{

}
public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(ClinicManagementContext context) : base(context)
    {
    }
}