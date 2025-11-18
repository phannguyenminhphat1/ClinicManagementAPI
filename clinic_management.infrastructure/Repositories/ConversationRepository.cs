using clinic_management.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IConversationRepository : IRepository<Conversation>
{
    public Task<Conversation?> GetConversationByConvId(int conversationId);
    public Task<Conversation?> GetConversationOfTwoUserId(Guid minId, Guid maxId);
}
public class ConversationRepository : Repository<Conversation>, IConversationRepository
{
    public ConversationRepository(ClinicManagementContext context) : base(context)
    {
    }

    public async Task<Conversation?> GetConversationByConvId(int conversationId)
    {
        var conversation = await _dbSet.Include(c => c.Messages.OrderBy(m => m.CreatedAt)).FirstOrDefaultAsync(c => c.ConversationId == conversationId);
        return conversation;
    }

    public async Task<Conversation?> GetConversationOfTwoUserId(Guid minId, Guid maxId)
    {
        var conversation = await _dbSet.Include(c => c.Messages.OrderBy(m => m.CreatedAt)).FirstOrDefaultAsync(c => c.UserMinId == minId && c.UserMaxId == maxId || c.UserMinId == maxId && c.UserMaxId == minId);
        return conversation;
    }


}