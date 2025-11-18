using clinic_management.infrastructure.Models;

public interface IPaymentMethodRepository : IRepository<PaymentMethod>
{

}
public class PaymentMethodRepository : Repository<PaymentMethod>, IPaymentMethodRepository
{
    public PaymentMethodRepository(ClinicManagementContext context) : base(context)
    {
    }
}