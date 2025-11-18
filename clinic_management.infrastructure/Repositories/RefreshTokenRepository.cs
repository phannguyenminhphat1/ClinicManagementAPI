using clinic_management.infrastructure.Models;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task DeleteRefreshTokenStringAsync(Guid refreshToken);

}
public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ClinicManagementContext context) : base(context)
    {

    }
    public async Task DeleteRefreshTokenStringAsync(Guid refreshToken)
    {
        var entity = await _dbSet.FindAsync(refreshToken);
        if (entity is not null)
        {
            _dbSet.Remove(entity);
        }
    }
}