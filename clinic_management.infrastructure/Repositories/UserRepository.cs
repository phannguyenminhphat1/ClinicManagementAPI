using System.Linq.Expressions;
using clinic_management.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetUserWithRoleAsync(Expression<Func<User, bool>> predicate);
    Task<List<User>> GetDoctorsPublicService(Expression<Func<User, bool>> predicate);
    public Task<List<User>> GetUsersByRoleService(int roleId);
    Task<User?> GetUserWithRoleAndSpecialtyAndMedicalRecord(Expression<Func<User, bool>> predicate);

    Task<(List<User> Users, int TotalRecords)> GetUsersForAdminAsync(
        int? roleId,
        string? name,
        string? phone,
        int? specialtyId,
        int page,
        int pageSize);
    Task<(List<User> Users, int TotalRecords)> GetUsersForReceptionistService(
        int roleGuest,
        byte? gender,
        string? keyword,
        int page,
        int pageSize);

    Task<(List<User> Users, int TotalRecords)> GetUsersForDoctorService(IQueryable<Guid> query, byte? gender, string? keyword, int page, int pageSize);

}
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ClinicManagementContext context) : base(context)
    {
    }

    public async Task<(List<User> Users, int TotalRecords)> GetUsersForAdminAsync(int? roleId, string? name, string? phone, int? specialtyId, int page, int pageSize)
    {
        var query = _dbSet
            .Include(u => u.Role)
            .Include(u => u.Specialty)
            .AsQueryable();

        // Filter theo role
        if (roleId.HasValue)
            query = query.Where(u => u.RoleId == roleId.Value);

        // Filter theo tên
        if (!string.IsNullOrWhiteSpace(name))
        {
            var lowerName = name.Trim().ToLower();
            query = query.Where(u => u.Fullname!.ToLower().Contains(lowerName));
        }

        // Filter theo phone
        if (!string.IsNullOrWhiteSpace(phone))
            query = query.Where(u => u.Phone!.Contains(phone.Trim()));

        // Filter theo specialty
        if (specialtyId.HasValue)
            query = query.Where(u => u.SpecialtyId == specialtyId.Value);

        var totalRecords = await query.CountAsync();

        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalRecords);
    }

    public async Task<(List<User> Users, int TotalRecords)> GetUsersForReceptionistService(int roleGuest, byte? gender, string? keyword, int page, int pageSize)
    {
        var query = _dbSet.Include(u => u.Role).Include(u => u.MedicalRecord).AsQueryable();
        query = query.Where(u => u.RoleId == roleGuest);

        // Filter theo keyword
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.Trim().ToLower();

            query = query.Where(u =>
                u.Fullname!.ToLower().Contains(lowerKeyword) ||
                u.Phone!.Contains(lowerKeyword));
        }

        // Filter theo Gender
        if (gender.HasValue)
        {
            query = query.Where(u => u.Gender == gender);
        }

        var totalRecords = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalRecords);
    }

    public async Task<(List<User> Users, int TotalRecords)> GetUsersForDoctorService(IQueryable<Guid> userIds, byte? gender, string? keyword, int page, int pageSize)
    {
        var query = _dbSet
            .Include(u => u.MedicalRecord).ThenInclude(mr => mr!.MedicalRecordDetails)
            .Where(u => userIds.Contains(u.UserId) && u.MedicalRecord != null);

        // Filter theo keyword
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.Trim().ToLower();

            query = query.Where(u =>
                u.Fullname!.ToLower().Contains(lowerKeyword) ||
                u.Phone!.Contains(lowerKeyword));
        }

        // Filter theo Gender
        if (gender.HasValue)
        {
            query = query.Where(u => u.Gender == gender);
        }

        var totalRecords = await query.CountAsync();

        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalRecords);
    }
    public async Task<User?> GetUserWithRoleAsync(Expression<Func<User, bool>> predicate)
    {
        var user = await _dbSet.Include(u => u.Role).SingleOrDefaultAsync(predicate);
        return user;
    }

    public async Task<List<User>> GetDoctorsPublicService(Expression<Func<User, bool>> predicate)
    {
        var users = await _dbSet.Include(u => u.Specialty).Where(predicate).ToListAsync();
        return users;
    }
    public async Task<List<User>> GetUsersByRoleService(int roleId)
    {
        var users = await _dbSet.Include(u => u.Role).Where(u => u.RoleId == roleId).ToListAsync();
        return users;
    }

    public async Task<User?> GetUserWithRoleAndSpecialtyAndMedicalRecord(Expression<Func<User, bool>> predicate)
    {
        var user = await _dbSet.Include(u => u.Role).Include(u => u.Specialty).Include(u => u.MedicalRecord).SingleOrDefaultAsync(predicate);
        return user;
    }

}