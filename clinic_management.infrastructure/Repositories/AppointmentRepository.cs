using System.Linq.Expressions;
using System.Text.Json;
using clinic_management.infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;


public interface IAppointmentRepository : IRepository<Appointment>
{
    public IQueryable<Guid> GetUserIdsHasAppointmentWithDoctor(Guid currentUserId);

    Task<bool> checkHasAppointmentWithCurrentRoleDoctor(Guid userId, Guid currentUserId);

    Task<bool> AnyAsync(Expression<Func<Appointment, bool>> predicate);

    Task<List<Appointment>> GetAppointmentsByDateRange(DateTime startDay, DateTime endDay, Guid? userId, string role, string roleGuest, string roleDoctor);

    public Task<List<Appointment>> GetAppointments(Guid? currentUserId, string currentRoleName, string roleGuest, string roleDoctor);

    public Task<(List<Appointment> Appointments, int TotalRecords)> GetAppointmentsQuery(Guid? currentUserId, string currentRoleName, string roleGuest, string roleDoctor, Expression<Func<Appointment, bool>>? predicate, int page,
        int pageSize, DateTime? dateFilter = null, string? keyword = null, Guid? doctorId = null);


    public Task<Appointment?> GetAppointmentById(Expression<Func<Appointment, bool>> predicate, Guid? currentUserId, string currentRoleName, string roleGuest, string roleDoctor);

    public Task<List<Appointment>> GetAppointmentsDoctorAsync(Expression<Func<Appointment, bool>> predicate);

    public Task<object> GetDoctorAppointmentStatistic(Guid? currentUserId, string currentRoleName, string roleGuest, string roleDoctor, int? page, int? pageSize, int completedStatus, int pendingStatus, int cancelledStatus, int examiningStatus, int? groupType, int? offset = 0);


}
public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(ClinicManagementContext context) : base(context)
    {
    }

    #region ANY ASYNC
    public async Task<bool> AnyAsync(Expression<Func<Appointment, bool>> predicate) => await _dbSet.AsNoTracking().AnyAsync(predicate);

    #endregion

    #region CHECK HAS APPOINTMENT WITH CURRENT ROLE DOCTOR
    public async Task<bool> checkHasAppointmentWithCurrentRoleDoctor(Guid userId, Guid currentUserId)
    {
        bool hasAppointment = await _dbSet.AnyAsync(a => a.PatientId == userId && a.DoctorId == currentUserId);
        return hasAppointment;
    }

    #endregion

    #region GET USER IDS HAVE APPOINTMENT WITH DOCTOR

    public IQueryable<Guid> GetUserIdsHasAppointmentWithDoctor(Guid currentUserId)
    {
        var query = _dbSet
            .Include(a => a.Patient)
            .Where(a => a.DoctorId == currentUserId && a.Patient != null)
            .Select(a => a.PatientId!.Value)
            .Distinct()
            .AsQueryable();
        return query;
    }
    #endregion

    #region GET APPOINTMENTS BY DATE RANGE


    public async Task<List<Appointment>> GetAppointmentsByDateRange(DateTime startDay, DateTime endDay, Guid? currentUserId, string currentRoleName, string roleGuest, string roleDoctor)
    {
        var query = _dbSet
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d!.Specialty)
            .Include(a => a.Status)
            .Where(a => a.ScheduledDate >= startDay && a.ScheduledDate <= endDay).AsQueryable();

        if (currentRoleName == roleGuest)
        {
            query = query.Where(a => a.PatientId == currentUserId);
        }
        else if (currentRoleName == roleDoctor)
        {
            query = query.Where(a => a.DoctorId == currentUserId);
        }
        var appointments = await query
            .OrderBy(a => a.ScheduledDate)
            .ToListAsync();

        return appointments;
    }
    #endregion

    #region GET APPOINTMENTS

    public async Task<List<Appointment>> GetAppointments(Guid? currentUserId, string currentRoleName, string roleGuest, string roleDoctor)
    {
        var query = _dbSet
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d!.Specialty)
            .Include(a => a.Status).AsQueryable();

        if (currentRoleName == roleGuest)
        {
            query = query.Where(a => a.PatientId == currentUserId);
        }
        else if (currentRoleName == roleDoctor)
        {
            query = query.Where(a => a.DoctorId == currentUserId);
        }
        var appointments = await query
            .OrderBy(a => a.ScheduledDate)
            .ToListAsync();

        return appointments;
    }
    #endregion

    #region GET APPOINTMENT BY ID


    public async Task<Appointment?> GetAppointmentById(Expression<Func<Appointment, bool>> predicate, Guid? currentUserId, string currentRoleName, string roleGuest, string roleDoctor)
    {
        var query = _dbSet
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d!.Specialty)
            .Include(a => a.Status)
            .AsQueryable();

        if (currentRoleName == roleGuest)
        {
            query = query.Where(a => a.PatientId == currentUserId);
        }
        else if (currentRoleName == roleDoctor)
        {
            query = query.Where(a => a.DoctorId == currentUserId);
        }

        var appointment = await query.SingleOrDefaultAsync(predicate);

        return appointment;
    }
    #endregion

    #region GET APPOINTMENTS DOCTOR
    public async Task<List<Appointment>> GetAppointmentsDoctorAsync(Expression<Func<Appointment, bool>> predicate)
    {
        return await _dbSet
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Status)
            .Where(predicate)
            .AsNoTracking()
            .ToListAsync();
    }
    #endregion

    #region GET APPOINTMENTS QUERY
    public async Task<(List<Appointment> Appointments, int TotalRecords)> GetAppointmentsQuery(Guid? currentUserId, string currentRoleName, string roleGuest, string roleDoctor, Expression<Func<Appointment, bool>>? predicate, int page, int pageSize, DateTime? dateFilter = null, string? keyword = null, Guid? doctorId = null)
    {
        var query = _dbSet
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d!.Specialty)
            .Include(a => a.Status)
            .AsQueryable();

        if (currentRoleName == roleGuest)
        {
            query = query.Where(a => a.PatientId == currentUserId);
        }
        else if (currentRoleName == roleDoctor)
        {
            query = query.Where(a => a.DoctorId == currentUserId);
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        if (dateFilter.HasValue)
        {
            var startTime = dateFilter.Value.Date.AddHours(8);
            var endTime = dateFilter.Value.Date.AddHours(17);
            query = query.Where(a => a.ScheduledDate >= startTime && a.ScheduledDate <= endTime);

        }
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.Trim().ToLower();
            query = query.Where(a =>
                a.Patient!.Fullname!.ToLower().Contains(lowerKeyword) ||
                a.Patient.Phone!.Contains(lowerKeyword));
        }

        if (!string.IsNullOrWhiteSpace(doctorId.ToString()))
        {
            query = query.Where(a => a.Doctor!.UserId == doctorId);
        }

        var totalRecords = await query.CountAsync();

        var appointments = await query
            .OrderByDescending(a => a.ScheduledDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (appointments, totalRecords);
    }

    #endregion

    #region GET DOCTOR APPOINTMENT STATISTIC
    public async Task<object> GetDoctorAppointmentStatistic(Guid? currentUserId, string currentRoleName, string roleGuest, string roleDoctor, int? page, int? pageSize, int completedStatus, int pendingStatus, int cancelledStatus, int examiningStatus, int? groupType, int? offset = 0)
    {
        var query = _dbSet
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Status)
            .AsQueryable();

        if (currentRoleName == roleGuest)
        {
            query = query.Where(a => a.PatientId == currentUserId);
        }
        else if (currentRoleName == roleDoctor)
        {
            query = query.Where(a => a.DoctorId == currentUserId);
        }

        List<object> groupData = new();
        int totalAppointments = 0, completed = 0, pending = 0, cancelled = 0, examining = 0;

        var today = DateTime.UtcNow;

        switch (groupType)
        {
            // case 1:
            //     {
            //         // Ngày được chọn = hôm nay + offset (mỗi offset = 1 ngày)
            //         var targetDate = today.AddDays(offset ?? 0);

            //         var groupedQueryDay = await query
            //             .Where(a => a.ScheduledDate.Date == targetDate.Date)
            //             .GroupBy(a => a.ScheduledDate.Date)
            //             .Select(g => new
            //             {
            //                 Date = g.Key,
            //                 Total = g.Count(),
            //                 Completed = g.Count(x => x.StatusId == completedStatus),
            //                 Pending = g.Count(x => x.StatusId == pendingStatus),
            //                 Cancelled = g.Count(x => x.StatusId == cancelledStatus),
            //                 Examining = g.Count(x => x.StatusId == examiningStatus),
            //             })
            //             .OrderBy(x => x.Date)
            //             .ToListAsync();

            //         totalAppointments = groupedQueryDay.Sum(x => x.Total);
            //         completed = groupedQueryDay.Sum(x => x.Completed);
            //         pending = groupedQueryDay.Sum(x => x.Pending);
            //         cancelled = groupedQueryDay.Sum(x => x.Cancelled);
            //         examining = groupedQueryDay.Sum(x => x.Examining);


            //         groupData = new List<object>
            //         {
            //             new
            //             {
            //                 labels = groupedQueryDay.Select(x => x.Date.ToString("dd/MM/yyyy")).ToList(),
            //                 datasets = new[]
            //                 {
            //                     new
            //                     {
            //                         label = targetDate.ToString("dd/MM/yyyy"),
            //                         data = groupedQueryDay.Select(x => x.Total).ToList(),
            //                         borderWidth = 1
            //                     }
            //                 },
            //                 details = groupedQueryDay.Select(x => new
            //                 {
            //                     date = x.Date.ToString("dd/MM/yyyy"),
            //                     total = x.Total,
            //                     completed = x.Completed,
            //                     pending = x.Pending,
            //                     cancelled = x.Cancelled,
            //                     examining = x.Examining

            //                 }).ToList()
            //             }
            //         };
            //         break;
            //     }

            case 2:
                {

                    var baseDate = today.AddDays((offset ?? 0) * 7);

                    // Tính ngày bắt đầu tuần (giả sử tuần bắt đầu từ thứ Hai)
                    var diffToMonday = (7 + (baseDate.DayOfWeek - DayOfWeek.Monday)) % 7;
                    var startOfWeek = baseDate.AddDays(-diffToMonday).Date;
                    var endOfWeek = startOfWeek.AddDays(7).Date;

                    var groupedQueryWeek = await query
                        .Where(x => x.ScheduledDate >= startOfWeek && x.ScheduledDate < endOfWeek)
                        .GroupBy(x => x.ScheduledDate.Date)
                        .Select(g => new
                        {
                            WeekLabel = $"{startOfWeek:dd/MM} - {endOfWeek.AddDays(-1):dd/MM}",
                            Date = g.Key,
                            Total = g.Count(),
                            Completed = g.Count(x => x.StatusId == completedStatus),
                            Pending = g.Count(x => x.StatusId == pendingStatus),
                            Cancelled = g.Count(x => x.StatusId == cancelledStatus),
                            Examining = g.Count(x => x.StatusId == examiningStatus),
                        })
                        .ToListAsync();

                    totalAppointments = groupedQueryWeek.Sum(x => x.Total);
                    completed = groupedQueryWeek.Sum(x => x.Completed);
                    pending = groupedQueryWeek.Sum(x => x.Pending);
                    cancelled = groupedQueryWeek.Sum(x => x.Cancelled);
                    examining = groupedQueryWeek.Sum(x => x.Examining);

                    var fullWeek = Enumerable.Range(0, 7)
                        .Select(i => startOfWeek.AddDays(i))
                        .Select(d => new
                        {
                            Date = d,
                            Total = groupedQueryWeek.FirstOrDefault(x => x.Date == d)?.Total ?? 0,
                            Completed = groupedQueryWeek.FirstOrDefault(x => x.Date == d)?.Completed ?? 0,
                            Pending = groupedQueryWeek.FirstOrDefault(x => x.Date == d)?.Pending ?? 0,
                            Cancelled = groupedQueryWeek.FirstOrDefault(x => x.Date == d)?.Cancelled ?? 0,
                            Examining = groupedQueryWeek.FirstOrDefault(x => x.Date == d)?.Examining ?? 0

                        })
                        .ToList();

                    var weekLabel = groupedQueryWeek.FirstOrDefault()?.WeekLabel ?? $"{startOfWeek:dd/MM} - {endOfWeek.AddDays(-1):dd/MM}";

                    groupData = new List<object>
                        {
                            new
                            {
                                labels = fullWeek.Select(x => x.Date.ToString("dd/MM")).ToList(),
                                datasets = new[]
                                {
                                    new
                                    {
                                        label = weekLabel,
                                        data = fullWeek.Select(x => x.Total).ToList(),
                                        borderWidth = 1
                                    }
                                },
                                details = fullWeek.Select(x => new
                                {
                                    date = x.Date.ToString("dd/MM/yyyy"),
                                    total = x.Total,
                                    completed = x.Completed,
                                    pending = x.Pending,
                                    cancelled = x.Cancelled,
                                    examining = x.Examining

                                }).ToList()
                            }
                        };
                    break;
                }

            case 3:
                {
                    // Xác định tháng cần lấy (theo offset)
                    var targetMonth = today.AddMonths(offset ?? 0);
                    var startOfMonth = new DateTime(targetMonth.Year, targetMonth.Month, 1);
                    var endOfMonth = startOfMonth.AddMonths(1);

                    // Query lấy toàn bộ lịch hẹn trong tháng đó
                    var groupedQueryMonth = await query
                        .Where(a => a.ScheduledDate >= startOfMonth && a.ScheduledDate < endOfMonth)
                        .GroupBy(a => EF.Functions.DateDiffWeek(startOfMonth, a.ScheduledDate))
                        .Select(g => new
                        {
                            WeekIndex = g.Key, // số tuần tính từ đầu tháng
                            Total = g.Count(),
                            Completed = g.Count(x => x.StatusId == completedStatus),
                            Pending = g.Count(x => x.StatusId == pendingStatus),
                            Cancelled = g.Count(x => x.StatusId == cancelledStatus),
                            Examining = g.Count(x => x.StatusId == examiningStatus)
                        })
                        .OrderBy(x => x.WeekIndex)
                        .ToListAsync();

                    // Tổng cộng chung
                    totalAppointments = groupedQueryMonth.Sum(x => x.Total);
                    completed = groupedQueryMonth.Sum(x => x.Completed);
                    pending = groupedQueryMonth.Sum(x => x.Pending);
                    cancelled = groupedQueryMonth.Sum(x => x.Cancelled);
                    examining = groupedQueryMonth.Sum(x => x.Examining);


                    // Tính tổng số tuần trong tháng
                    var daysInMonth = DateTime.DaysInMonth(targetMonth.Year, targetMonth.Month);
                    var totalWeeks = (int)Math.Ceiling(daysInMonth / 7.0);

                    var fullMonth = Enumerable.Range(0, totalWeeks)
                    .Select(i =>
                    {
                        var start = startOfMonth.AddDays(i * 7);
                        var end = start.AddDays(6);

                        var data = groupedQueryMonth.FirstOrDefault(x => x.WeekIndex == i);

                        return new
                        {
                            WeekLabel = $"Tuần {i + 1} ({start:dd/MM} - {end:dd/MM})",
                            StartDate = start,
                            EndDate = end,
                            Total = data?.Total ?? 0,
                            Completed = data?.Completed ?? 0,
                            Pending = data?.Pending ?? 0,
                            Cancelled = data?.Cancelled ?? 0,
                            Examining = data?.Examining ?? 0

                        };
                    })
                    .ToList();


                    var monthLabel = $"{targetMonth:MM/yyyy}";

                    groupData = new List<object>
                    {
                        new
                        {
                            labels = fullMonth.Select(x => x.WeekLabel).ToList(),
                            datasets = new[]
                            {
                                new
                                {
                                    label = monthLabel,
                                    data = fullMonth.Select(x => x.Total).ToList(),
                                    borderWidth = 1
                                }
                            },
                            details = fullMonth.Select(x => new
                            {
                                week = x.WeekLabel,
                                total = x.Total,
                                completed = x.Completed,
                                pending = x.Pending,
                                cancelled = x.Cancelled,
                                examining = x.Examining
                            }).ToList()

                        }
                    };
                    break;
                }

            default:
                goto case 2;
        }

        var result = new
        {
            summary = new
            {
                totalAppointments,
                completed,
                pending,
                cancelled,
                examining
            },
            chart = groupData
        };

        return result;
    }

    #endregion


}