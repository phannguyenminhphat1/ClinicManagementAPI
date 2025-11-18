using clinic_management.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IMessageRepository : IRepository<Message>
{
    public Task<(List<Message> ListMessages, int TotalRecords)> GetMessagesByConvId(int convId, int page, int pageSize);
}
public class MessageRepository : Repository<Message>, IMessageRepository
{
    public MessageRepository(ClinicManagementContext context) : base(context)
    {
    }

    public async Task<(List<Message> ListMessages, int TotalRecords)> GetMessagesByConvId(int convId, int page, int pageSize)
    {
        var query = _dbSet.Where(m => m.ConversationId == convId).AsQueryable();
        var totalRecords = await query.CountAsync();
        var lstMessages = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (lstMessages, totalRecords);
    }

}