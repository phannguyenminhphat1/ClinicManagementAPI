using clinic_management.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IServiceRepository : IRepository<Service>
{
    public Task<Service?> GetEntryService();
    public Task<List<Service>?> GetAllServices();


}
public class ServiceRepository : Repository<Service>, IServiceRepository
{
    public ServiceRepository(ClinicManagementContext context) : base(context)
    {
    }

    public async Task<Service?> GetEntryService()
    {
        var entryService = await _dbSet.FirstOrDefaultAsync(e => e.ServiceName == "Khám đầu vào");
        if (entryService == null)
        {
            return null;
        }
        return entryService;

    }
    public async Task<List<Service>?> GetAllServices()
    {
        var services = await _dbSet.Where(e => e.ServiceName != "Khám đầu vào").ToListAsync();
        return services;

    }
}