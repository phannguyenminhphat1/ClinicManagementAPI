using clinic_management.infrastructure.Models;

public interface IPaymentDetailRepository : IRepository<PaymentDetail>
{

}
public class PaymentDetailRepository : Repository<PaymentDetail>, IPaymentDetailRepository
{
    public PaymentDetailRepository(ClinicManagementContext context) : base(context)
    {
    }
}