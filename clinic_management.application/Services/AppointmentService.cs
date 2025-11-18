using System.Linq.Expressions;
using System.Net;
using AutoMapper;
using clinic_management.infrastructure.Models;
using Microsoft.AspNetCore.Identity;


public interface IAppointmentService
{
    public Task<ResponseService<object>> MakeAppointmentService(MakeAppointmentDto makeAppointmentDto);
    public Task<ResponseService<object>> MakeAppointmentWithDoctorService(MakeAppointmentWithDoctorDto makeAppointmentWithDoctorDto);
    public Task<ResponseService<List<GetAppointmentDto>>> GetAppointmentsByDateService(Guid currentUserId, AppointmentFilterDto appointmentFilterDto);
    public Task<ResponseService<ResponsePagedService<List<GetAppointmentDto>>>> GetAppointmentsService(Guid currentUserId, PaginationDto paginationDto, GetAppointmentsFilterDto dto);
    public Task<ResponseService<List<AvailableTimeDto>>> GetAvailableTimeSlotsService(DateTimeDto dto);
    public Task<ResponseService<List<AvailableTimeDto>>> GetAvailableTimeSlotsDoctorService(string date, string doctorId, bool allowCheck = true);
    public Task<ResponseService<GetAppointmentDto>> GetAppointmentByIdService(Guid currentUserId, string appointmentId);
    public Task<ResponseService<object>> UpdateAppointmentService(Guid currentUserId, UpdateAppointmentDto dto, string appointmentId);
    public Task<ResponseService<object>> UpdateAppointmentStatusService(Guid currentUserId, UpdateAppointmentStatusDto dto, string appointmentId);
    public Task<ResponseService<object>> UpdateAppointmentDagService(Guid currentUserId, UpdateAppointmentStatusDto dto, string appointmentId);
    public Task<ResponseService<string>> CheckInService(Guid currentUserId, string appointmentId);
    public Task<ResponseService<List<AppointmentStatusHistoryDto>>> GetAppointmentHistoriesService(Guid currentUserId, string appointmentId);

}

public class AppointmentService(IConfiguration _configuration, IUserRepository userRepo, IAppointmentRepository appointmentRepo, IUnitOfWork unitOfWork, IUserService userService, IMapper _mapper, IAppointmentStatusHistoryRepository appointmentStatusHistoryRepo, IBillingRepository billingRepo, IBillingDetailRepository billingDetailRepo, IServiceRepository serviceRepo) : IAppointmentService
{

    #region MAKE APPOINTMENT
    public async Task<ResponseService<object>> MakeAppointmentService(MakeAppointmentDto makeAppointmentDto)
    {
        var errors = new Dictionary<string, string>();
        DateTime? scheduledDateTime = null;
        DateTime parsedDate = default;
        TimeSpan parsedTime = default;

        // 1. Validate Phone (cái này luôn required theo DTO)
        if (string.IsNullOrWhiteSpace(makeAppointmentDto.Phone))
        {
            errors["phone"] = AuthMessages.PHONE_IS_REQUIRED;
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(makeAppointmentDto.Phone, @"^(0[3|5|7|8|9])[0-9]{8}$"))
        {
            errors["phone"] = UserMessages.PHONE_IS_INVALID;
        }

        // Validate ngày hẹn
        if (string.IsNullOrWhiteSpace(makeAppointmentDto.Date))
        {
            errors["date"] = AppointmentMessages.DATE_IS_REQUIRED;
        }
        else if (!DateTime.TryParseExact(makeAppointmentDto.Date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out parsedDate))
        {
            errors["date"] = AppointmentMessages.DATE_IS_INVALID;
        }

        // Validate giờ hẹn và Ghép Date và Time lại thành ScheduledDate
        if (string.IsNullOrWhiteSpace(makeAppointmentDto.Time))
        {
            errors["time"] = AppointmentMessages.TIME_IS_REQUIRED;
        }
        else if (!TimeSpan.TryParse(makeAppointmentDto.Time, out parsedTime))
        {
            errors["time"] = AppointmentMessages.TIME_IS_INVALID;
        }

        if (!errors.ContainsKey("date") && !errors.ContainsKey("time"))
        {
            var scheduledDateTimeCombine = parsedDate.Date.Add(parsedTime);
            if (scheduledDateTimeCombine < DateTime.UtcNow)
            {
                errors["date"] = AppointmentMessages.SCHEDULED_DATE_IS_INVALID;
            }
            else
            {
                scheduledDateTime = scheduledDateTimeCombine;
            }
        }

        if (!makeAppointmentDto.IsExistingPatient)
        {
            // Fullname
            if (string.IsNullOrWhiteSpace(makeAppointmentDto.Fullname))
            {
                errors["fullname"] = AuthMessages.FULLNAME_IS_REQUIRED;
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(makeAppointmentDto.Fullname, @"^[A-Za-zÀ-ỹ\s]{2,50}$"))
            {
                errors["fullname"] = UserMessages.FULL_NAME_IS_INVALID;
            }

            // Gender
            if (!makeAppointmentDto.Gender.HasValue)
            {
                errors["gender"] = AuthMessages.GENDER_IS_REQUIRED;
            }
            else if (makeAppointmentDto.Gender < 0 || makeAppointmentDto.Gender > 1)
            {
                errors["gender"] = AuthMessages.GENDER_IS_INVALID;
            }

            // BirthDate
            if (string.IsNullOrWhiteSpace(makeAppointmentDto.BirthDate))
            {
                errors["birth_date"] = AuthMessages.BIRTH_DATE_IS_REQUIRED;
            }
            else if (!DateOnly.TryParseExact(makeAppointmentDto.BirthDate, "yyyy-MM-dd", out _))
            {
                errors["birth_date"] = AuthMessages.BIRTH_DATE_IS_INVALID;
            }
        }

        if (errors.Count > 0)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.UnprocessableEntity,
                message: AppointmentMessages.ERROR,
                errors: errors
            );
        }

        // Kiểm tra lịch hẹn của các bác sĩ và lấy về danh sách bác sĩ
        var availableDoctors = await GetAvailableDoctorsAtTimeService(scheduledDateTime!.Value);
        if (availableDoctors.Count == 0)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: AppointmentMessages.NO_DOCTORS_AVAILABLE_AT_THIS_TIME
            );
        }

        // Nếu ổn hết rồi thì tiến hành các bước ở dưới thôi
        // Kiểm tra có phải từng là bệnh nhân cũ không
        var existingUser = await userRepo.GetUserWithRoleAndSpecialtyAndMedicalRecord(u => u.Phone == makeAppointmentDto.Phone);

        if (makeAppointmentDto.IsExistingPatient) // bệnh nhân cũ
        {
            if (existingUser == null)
            {
                return new ResponseService<object>(
                    statusCode: (int)HttpStatusCode.NotFound,
                    message: AppointmentMessages.USER_NOT_FOUND
                );
            }

        }
        else
        {
            // Kiểm tra phone đã tồn tại chưa (nếu tồn tại, yêu cầu chọn "Đã từng khám")
            if (existingUser != null)
            {
                return new ResponseService<object>(
                    statusCode: (int)HttpStatusCode.BadRequest,
                    message: AppointmentMessages.PHONE_IS_ALREADY_EXISTED
                );
            }

            // Tạo user mới
            var parsedBirthDate = DateOnly.ParseExact(makeAppointmentDto.BirthDate!, "yyyy-MM-dd");
            existingUser = new User
            {
                Fullname = makeAppointmentDto.Fullname,
                Phone = makeAppointmentDto.Phone,
                Password = new PasswordHasher<User>().HashPassword(new User(), "abc"),
                Gender = makeAppointmentDto.Gender,
                BirthDate = parsedBirthDate,
                Weight = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Image = _configuration["AppSettings:CloudinaryDefaultAvt"],
                RoleId = (int)UserRole.Guest
            };
            await userRepo.AddAsync(existingUser);
        }


        // Random 1 bác sĩ
        var random = new Random();
        var randomDoctor = availableDoctors[random.Next(availableDoctors.Count)];

        // Tạo lịch hẹn
        bool alreadyBooked = await CheckAlreadyBooked(scheduledDateTime.Value, existingUser);
        if (alreadyBooked)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: AppointmentMessages.ALREADY_HAVE_AN_APPOINTMENT
            );
        }
        // Đặt lịch thành công
        var appointment = new Appointment
        {
            Patient = existingUser,
            DoctorId = randomDoctor.UserId,
            ScheduledDate = scheduledDateTime.Value,
            StatusId = (int)AppointmentStatus.Scheduled, // Đã đặt lịch
            Note = makeAppointmentDto.Note ?? "",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await appointmentRepo.AddAsync(appointment);

        // Add vào bảng History
        var appointmentStatusHistory = new AppointmentStatusHistory
        {
            StatusId = (int)AppointmentStatus.Scheduled,
            Appointment = appointment,
            Note = AppointmentMessages.MAKE_APPOINTMENT_SUCCESSFULLY
        };
        await appointmentStatusHistoryRepo.AddAsync(appointmentStatusHistory);

        await unitOfWork.SaveChangesAsync();
        return new ResponseService<object>(
            statusCode: (int)HttpStatusCode.Created,
            message: AppointmentMessages.MAKE_APPOINTMENT_SUCCESSFULLY
        );
    }

    #endregion

    #region MAKE APPOINTMENT WITH DOCTOR
    public async Task<ResponseService<object>> MakeAppointmentWithDoctorService(MakeAppointmentWithDoctorDto makeAppointmentWithDoctorDto)
    {
        var errors = new Dictionary<string, string>();
        DateTime? scheduledDateTime = null;
        DateTime parsedDate = default;
        TimeSpan parsedTime = default;
        Guid parsedDoctorId = default;

        // Validate Phone (cái này luôn required theo DTO)
        if (string.IsNullOrWhiteSpace(makeAppointmentWithDoctorDto.Phone))
        {
            errors["phone"] = AuthMessages.PHONE_IS_REQUIRED;
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(makeAppointmentWithDoctorDto.Phone, @"^(0[3|5|7|8|9])[0-9]{8}$"))
        {
            errors["phone"] = UserMessages.PHONE_IS_INVALID;
        }

        // Validate doctorId
        if (string.IsNullOrWhiteSpace(makeAppointmentWithDoctorDto.DoctorId))
        {
            errors["doctor_id"] = AppointmentMessages.DOCTOR_ID_IS_REQUIRED;
        }
        else if (!Guid.TryParse(makeAppointmentWithDoctorDto.DoctorId, out parsedDoctorId))
        {
            errors["doctor_id"] = AppointmentMessages.DOCTOR_ID_IS_INVALID;
        }

        // Validate ngày hẹn
        if (string.IsNullOrWhiteSpace(makeAppointmentWithDoctorDto.Date))
        {
            errors["date"] = AppointmentMessages.DATE_IS_REQUIRED;
        }
        else if (!DateTime.TryParseExact(makeAppointmentWithDoctorDto.Date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out parsedDate))
        {
            errors["date"] = AppointmentMessages.DATE_IS_INVALID;
        }

        // Validate giờ hẹn và Ghép Date và Time lại thành ScheduledDate
        if (string.IsNullOrWhiteSpace(makeAppointmentWithDoctorDto.Time))
        {
            errors["time"] = AppointmentMessages.TIME_IS_REQUIRED;
        }
        else if (!TimeSpan.TryParse(makeAppointmentWithDoctorDto.Time, out parsedTime))
        {
            errors["time"] = AppointmentMessages.TIME_IS_INVALID;
        }

        if (!errors.ContainsKey("date") && !errors.ContainsKey("time"))
        {
            var scheduledDateTimeCombine = parsedDate.Date.Add(parsedTime);
            if (scheduledDateTimeCombine < DateTime.UtcNow)
            {
                errors["date"] = AppointmentMessages.SCHEDULED_DATE_IS_INVALID;
            }
            else
            {
                scheduledDateTime = scheduledDateTimeCombine;
            }
        }
        if (!makeAppointmentWithDoctorDto.IsExistingPatient)
        {
            // Fullname
            if (string.IsNullOrWhiteSpace(makeAppointmentWithDoctorDto.Fullname))
            {
                errors["fullname"] = AuthMessages.FULLNAME_IS_REQUIRED;
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(makeAppointmentWithDoctorDto.Fullname, @"^[A-Za-zÀ-ỹ\s]{2,50}$"))
            {
                errors["fullname"] = UserMessages.FULL_NAME_IS_INVALID;
            }

            // Gender
            if (!makeAppointmentWithDoctorDto.Gender.HasValue)
            {
                errors["gender"] = AuthMessages.GENDER_IS_REQUIRED;
            }
            else if (makeAppointmentWithDoctorDto.Gender < 0 || makeAppointmentWithDoctorDto.Gender > 1)
            {
                errors["gender"] = AuthMessages.GENDER_IS_INVALID;
            }

            // BirthDate
            if (string.IsNullOrWhiteSpace(makeAppointmentWithDoctorDto.BirthDate))
            {
                errors["birth_date"] = AuthMessages.BIRTH_DATE_IS_REQUIRED;
            }
            else if (!DateOnly.TryParseExact(makeAppointmentWithDoctorDto.BirthDate, "yyyy-MM-dd", out _))
            {
                errors["birth_date"] = AuthMessages.BIRTH_DATE_IS_INVALID;
            }
        }

        if (errors.Count > 0)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.UnprocessableEntity,
                message: AppointmentMessages.ERROR,
                errors: errors
            );
        }



        var getAvailableTimeSlotsDoctor = await GetAvailableTimeSlotsDoctorService(parsedDate.ToString(), parsedDoctorId.ToString(), false);

        // Check xem slot có tồn tại trong danh sách available không
        var matchedSlot = getAvailableTimeSlotsDoctor.Data?
            .FirstOrDefault(s => s.Time == parsedTime.ToString(@"hh\:mm"));

        if (matchedSlot is null)
        {
            errors["time"] = AppointmentMessages.TIME_SLOT_NOT_FOUND;
        }
        else if (!matchedSlot.IsAvailable)
        {
            errors["time"] = AppointmentMessages.TIME_SLOT_IS_NOT_AVAILABLE;
        }

        // Nếu có lỗi thì return luôn
        if (errors.Count > 0)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.UnprocessableEntity,
                message: AppointmentMessages.ERROR,
                errors: errors
            );
        }

        /// Nếu ổn hết rồi thì tiến hành các bước ở dưới thôi
        // Kiểm tra có phải từng là bệnh nhân cũ không
        var existingUser = await userRepo.GetUserWithRoleAndSpecialtyAndMedicalRecord(u => u.Phone == makeAppointmentWithDoctorDto.Phone);

        if (makeAppointmentWithDoctorDto.IsExistingPatient) // bệnh nhân cũ
        {
            if (existingUser == null)
            {
                return new ResponseService<object>(
                    statusCode: (int)HttpStatusCode.NotFound,
                    message: AppointmentMessages.USER_NOT_FOUND
                );
            }

        }
        else
        {
            // Kiểm tra phone đã tồn tại chưa (nếu tồn tại, yêu cầu chọn "Đã từng khám")
            if (existingUser != null)
            {
                return new ResponseService<object>(
                    statusCode: (int)HttpStatusCode.BadRequest,
                    message: AppointmentMessages.PHONE_IS_ALREADY_EXISTED
                );
            }

            // Tạo user mới
            var parsedBirthDate = DateOnly.ParseExact(makeAppointmentWithDoctorDto.BirthDate!, "yyyy-MM-dd");
            existingUser = new User
            {
                Fullname = makeAppointmentWithDoctorDto.Fullname,
                Phone = makeAppointmentWithDoctorDto.Phone,
                Password = new PasswordHasher<User>().HashPassword(new User(), "abc"),
                Gender = makeAppointmentWithDoctorDto.Gender,
                BirthDate = parsedBirthDate,
                Weight = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Image = _configuration["AppSettings:CloudinaryDefaultAvt"],
                RoleId = (int)UserRole.Guest
            };
            await userRepo.AddAsync(existingUser);
        }

        // Tạo lịch hẹn
        bool alreadyBooked = await CheckAlreadyBooked(scheduledDateTime!.Value, existingUser);
        if (alreadyBooked)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: AppointmentMessages.ALREADY_HAVE_AN_APPOINTMENT
            );
        }
        // Đặt lịch thành công
        var appointment = new Appointment
        {
            Patient = existingUser,
            DoctorId = parsedDoctorId,
            ScheduledDate = scheduledDateTime.Value,
            StatusId = (int)AppointmentStatus.Scheduled, // Đã đặt lịch
            Note = makeAppointmentWithDoctorDto.Note ?? "",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await appointmentRepo.AddAsync(appointment);

        // Add vào bảng History
        var appointmentStatusHistory = new AppointmentStatusHistory
        {
            StatusId = (int)AppointmentStatus.Scheduled,
            Appointment = appointment,
            Note = AppointmentMessages.MAKE_APPOINTMENT_SUCCESSFULLY
        };
        await appointmentStatusHistoryRepo.AddAsync(appointmentStatusHistory);


        await unitOfWork.SaveChangesAsync();
        return new ResponseService<object>(
            statusCode: (int)HttpStatusCode.Created,
            message: AppointmentMessages.MAKE_APPOINTMENT_SUCCESSFULLY
        );
    }

    #endregion

    #region GET AVAILABLE DOCTORS AT TIME
    public async Task<List<DoctorDto>> GetAvailableDoctorsAtTimeService(DateTime scheduledDate)
    {
        DateTime endTime = scheduledDate.AddMinutes(30);

        var doctors = await userService.GetDoctorsPublicService();
        List<DoctorDto> availableDoctors = new List<DoctorDto>();
        foreach (var doctor in doctors.Data!)
        {
            bool isBusy = await appointmentRepo.AnyAsync(d =>
                d.DoctorId == doctor.UserId &&
                d.StatusId != (int)AppointmentStatus.Canceled &&
                d.ScheduledDate < endTime &&
                d.ScheduledDate.AddMinutes(30) > scheduledDate
            );
            if (!isBusy)
            {
                availableDoctors.Add(doctor);
            }
        }
        return availableDoctors;
    }
    #endregion


    #region CHECK ALREADY BOOKED
    // User,Receiptionist
    public async Task<bool> CheckAlreadyBooked(DateTime scheduledTime, User existingUser)
    {
        // Kiểm tra xem người dùng đã có lịch trong khoảng giờ đó chưa
        DateTime startTime = scheduledTime;
        DateTime endTime = startTime.AddMinutes(30);

        bool alreadyBooked = await appointmentRepo.AnyAsync(a =>
            a.PatientId == existingUser!.UserId &&
            a.StatusId != (int)AppointmentStatus.Canceled &&
            a.ScheduledDate < endTime &&
            a.ScheduledDate.AddMinutes(30) > startTime
        );
        return alreadyBooked;
    }
    #endregion


    #region GET AVAILABLE TIME SLOTS
    // User,Receiptionist
    public async Task<ResponseService<List<AvailableTimeDto>>> GetAvailableTimeSlotsService(DateTimeDto dto)
    {
        // Validate ngày hẹn
        if (!DateTime.TryParseExact(dto.Date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
        {
            return new ResponseService<List<AvailableTimeDto>>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: AppointmentMessages.DATE_IS_INVALID
            );
        }

        DateTime start = parsedDate.AddHours(8);     // 08:00
        DateTime end = parsedDate.AddHours(17);      // 17:00
        TimeSpan slot = TimeSpan.FromMinutes(30);

        var doctors = await userService.GetDoctorsPublicService();
        var results = new List<AvailableTimeDto>();

        for (DateTime time = start; time < end; time += slot)
        {
            DateTime slotEnd = time + slot;
            bool hasAvailableDoctor = false;

            foreach (var doctor in doctors.Data!)
            {
                bool busy = await appointmentRepo.AnyAsync(a =>
                    a.DoctorId == doctor.UserId &&
                    a.StatusId != (int)AppointmentStatus.Canceled &&
                    a.ScheduledDate < slotEnd &&
                    a.ScheduledDate.AddMinutes(30) > time
                );

                if (!busy)
                {
                    hasAvailableDoctor = true;
                    break; // chỉ cần 1 bác sĩ rảnh là break
                }
            }

            results.Add(new AvailableTimeDto
            {
                Time = time.ToString("HH:mm"),
                IsAvailable = hasAvailableDoctor
            });
        }

        return new ResponseService<List<AvailableTimeDto>>(
            statusCode: (int)HttpStatusCode.OK,
            message: AppointmentMessages.GET_AVAILABLE_TIMES,
            data: results
        );
    }
    #endregion


    #region GET AVAILABLE TIME SLOTS DOCTOR ID
    // User,Receiptionist
    public async Task<ResponseService<List<AvailableTimeDto>>> GetAvailableTimeSlotsDoctorService(string date, string doctorId, bool allowCheck)
    {
        DateTime parsedDate;
        Guid parsedDoctorId;

        if (allowCheck)
        {
            // Validate ngày
            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out parsedDate))
            {
                return new ResponseService<List<AvailableTimeDto>>(
                    statusCode: (int)HttpStatusCode.BadRequest,
                    message: AppointmentMessages.DATE_IS_INVALID
                );
            }
            // Validate doctorId
            if (!Guid.TryParse(doctorId, out parsedDoctorId))
            {
                return new ResponseService<List<AvailableTimeDto>>(
                    statusCode: (int)HttpStatusCode.BadRequest,
                    message: AppointmentMessages.DOCTOR_ID_IS_INVALID
                );
            }
        }
        else
        {
            parsedDate = DateTime.Parse(date);
            parsedDoctorId = Guid.Parse(doctorId);
        }

        DateTime start = parsedDate.AddHours(8);     // 08:00
        DateTime end = parsedDate.AddHours(17);      // 17:00
        TimeSpan slot = TimeSpan.FromMinutes(30);

        var doctor = await userRepo.GetUserWithRoleAsync(u => u.UserId == parsedDoctorId && u.Role!.RoleId == (int)UserRole.Doctor);
        if (doctor is null)
        {
            return new ResponseService<List<AvailableTimeDto>>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: AppointmentMessages.DOCTOR_NOT_FOUND
            );
        }
        // Lấy tất cả appointment của bác sĩ trong ngày
        var appointments = await appointmentRepo.GetAppointmentsDoctorAsync(a =>
            a.DoctorId == doctor.UserId &&
            a.StatusId != (int)AppointmentStatus.Canceled &&
            a.ScheduledDate.Date == parsedDate
        );
        var results = new List<AvailableTimeDto>();

        for (DateTime time = start; time < end; time += slot)
        {
            DateTime slotEnd = time + slot;
            bool busy = appointments.Any(a => a.ScheduledDate < slotEnd && a.ScheduledDate.AddMinutes(30) > time);
            results.Add(new AvailableTimeDto
            {
                Time = time.ToString("HH:mm"),
                IsAvailable = !busy
            });
        }

        return new ResponseService<List<AvailableTimeDto>>(
            statusCode: (int)HttpStatusCode.OK,
            message: AppointmentMessages.GET_AVAILABLE_TIMES,
            data: results
        );
    }
    #endregion


    #region GET APPOINTMENTS BY DATE
    // All Users
    public async Task<ResponseService<List<GetAppointmentDto>>> GetAppointmentsByDateService(Guid currentUserId, AppointmentFilterDto appointmentFilterDto)
    {
        DateTime startDay;
        DateTime endDay;

        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);

        if (currentUser == null)
        {
            return new ResponseService<List<GetAppointmentDto>>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }

        bool hasStartDay = !string.IsNullOrWhiteSpace(appointmentFilterDto.StartDay);
        bool hasEndDay = !string.IsNullOrWhiteSpace(appointmentFilterDto.EndDay);

        // Nếu có end_day nhưng không có start_day => lỗi
        if (hasEndDay && !hasStartDay || !hasEndDay && hasStartDay)
        {
            return new ResponseService<List<GetAppointmentDto>>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: AppointmentMessages.START_DAY_AND_END_DAY_IS_REQUIRED
            );
        }

        // Kiểm tra và parse start_day
        if (!hasStartDay)
        {
            startDay = DateTime.Today;
        }
        // Validate start day
        else if (!DateTime.TryParseExact(appointmentFilterDto.StartDay, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out startDay))
        {
            return new ResponseService<List<GetAppointmentDto>>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: AppointmentMessages.START_DATE_IS_INVALID
            );
        }

        // Kiểm tra và parse end_day
        if (!hasEndDay)
        {
            endDay = DateTime.Today.AddDays(1).AddTicks(-1);
        }
        else if (!DateTime.TryParseExact(appointmentFilterDto.EndDay, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out endDay))
        {
            return new ResponseService<List<GetAppointmentDto>>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: AppointmentMessages.END_DATE_IS_INVALID
            );
        }
        else
        {
            endDay = endDay.AddDays(1).AddTicks(-1);
        }

        // Check start_day phải bé hơn end_day
        if (startDay > endDay)
        {
            return new ResponseService<List<GetAppointmentDto>>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: AppointmentMessages.START_DATE_MUST_BE_LESS_THAN_END_DAY
            );
        }

        // query
        var appointments = await appointmentRepo.GetAppointmentsByDateRange(startDay, endDay, currentUserId, currentUser.Role!.RoleName, UserRolesName.Guest.ToString(), UserRolesName.Doctor.ToString());

        var result = _mapper.Map<List<GetAppointmentDto>>(appointments);
        return new ResponseService<List<GetAppointmentDto>>(
            statusCode: (int)HttpStatusCode.OK,
            message: AppointmentMessages.GET_APPOINTMENTS_SUCCESSFULLY,
            data: result
        );
    }
    #endregion


    #region GET APPOINTMENTS
    // All Users
    public async Task<ResponseService<ResponsePagedService<List<GetAppointmentDto>>>> GetAppointmentsService(Guid currentUserId, PaginationDto paginationDto, GetAppointmentsFilterDto dto)
    {

        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
        {
            return new ResponseService<ResponsePagedService<List<GetAppointmentDto>>>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }

        int? parsedStatusId = null;
        Expression<Func<Appointment, bool>>? predicate = null;

        DateTime? date = null;
        Guid? parsedDoctorId = null;


        // Nếu có truyền date
        if (!string.IsNullOrWhiteSpace(dto.Date))
        {
            if (!DateTime.TryParseExact(dto.Date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
            {
                return new ResponseService<ResponsePagedService<List<GetAppointmentDto>>>(
                    statusCode: (int)HttpStatusCode.BadRequest,
                    message: AppointmentMessages.FILTER_DATE_IS_INVALID
                );
            }
            date = parsedDate;
        }

        // Validate StatusId
        if (!string.IsNullOrWhiteSpace(dto.StatusId))
        {
            if (!ValidateAndParseStatus.TryParseStatus<ResponsePagedService<List<GetAppointmentDto>>>(dto.StatusId, out parsedStatusId, out var errorResponseStatus))
            {
                return errorResponseStatus!;
            }
            if (parsedStatusId.HasValue)
            {
                if (!EnumValidationService.IsValid<AppointmentStatus>(parsedStatusId.Value))
                {
                    return new ResponseService<ResponsePagedService<List<GetAppointmentDto>>>(
                        statusCode: (int)HttpStatusCode.OK,
                        message: $"{AppointmentMessages.STATUS_ID_IS_INVALID}: {EnumValidationService.GetValidEnumValues<AppointmentStatus>()}"
                    );
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.DoctorId))
        {
            if (!Guid.TryParse(dto.DoctorId, out var tempDoctorId))
            {
                return new ResponseService<ResponsePagedService<List<GetAppointmentDto>>>(
                    statusCode: (int)HttpStatusCode.BadRequest,
                    message: UserMessages.DOCTOR_ID_IS_INVALID
                );
            }
            parsedDoctorId = tempDoctorId;
        }

        if (!ValidateAndParsePagination.TryParsePagination<ResponsePagedService<List<GetAppointmentDto>>>(paginationDto.Page, paginationDto.PageSize, out int? pageParsed, out int? pageSizeParsed, out var errorResponsePaged))
        {
            return errorResponsePaged!;
        }

        if (parsedStatusId != null)
        {
            int statusIdValue = parsedStatusId.Value;
            predicate = a => a.Status!.StatusId == statusIdValue;
        }

        int currentPage = pageParsed ?? 1;
        int pageSize = pageSizeParsed ?? 10;

        // query
        var (appointments, totalRecords) = await appointmentRepo.GetAppointmentsQuery(currentUserId, currentUser.Role!.RoleName, UserRolesName.Guest.ToString(), UserRolesName.Doctor.ToString(), predicate, currentPage, pageSize, date, dto.Keyword, parsedDoctorId);

        int totalPage = (int)Math.Ceiling((double)totalRecords / pageSize);

        var appointmentsDto = _mapper.Map<List<GetAppointmentDto>>(appointments);

        var resultResponse = new ResponsePagedService<List<GetAppointmentDto>>(
            data: appointmentsDto,
            currentPage: currentPage,
            pageSize: pageSize,
            totalPage: totalPage,
            totalItem: totalRecords
        );

        return new ResponseService<ResponsePagedService<List<GetAppointmentDto>>>(
            statusCode: (int)HttpStatusCode.OK,
            message: AppointmentMessages.GET_APPOINTMENTS_SUCCESSFULLY,
            data: resultResponse
        );
    }
    #endregion

    #region GET APPOINTMENT BY ID
    public async Task<ResponseService<GetAppointmentDto>> GetAppointmentByIdService(Guid currentUserId, string appointmentId)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
        {
            return new ResponseService<GetAppointmentDto>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }

        if (!int.TryParse(appointmentId, out var parsedAppointmentId))
        {
            return new ResponseService<GetAppointmentDto>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: AppointmentMessages.APPOINTMENT_MUST_BE_A_NUMBER
            );
        }
        var appointment = await appointmentRepo.GetAppointmentById(a => a.AppointmentId == parsedAppointmentId, currentUserId, currentUser.Role!.RoleName, UserRolesName.Guest.ToString(), UserRolesName.Doctor.ToString());

        if (appointment is null)
        {
            return new ResponseService<GetAppointmentDto>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: AppointmentMessages.APPOINTMENT_NOT_FOUND
            );
        }
        var result = _mapper.Map<GetAppointmentDto>(appointment);

        return new ResponseService<GetAppointmentDto>(
           statusCode: (int)HttpStatusCode.OK,
           message: AppointmentMessages.GET_APPOINTMENT_SUCCESSFULLY,
           data: result
       );
    }

    #endregion


    #region UPDATE APPOINTMENT
    public async Task<ResponseService<object>> UpdateAppointmentService(Guid currentUserId, UpdateAppointmentDto dto, string appointmentId)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }
        if (!int.TryParse(appointmentId, out var parsedAppointmentId))
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: AppointmentMessages.APPOINTMENT_MUST_BE_A_NUMBER
            );
        }
        var appointment = await appointmentRepo.GetAppointmentById(a => a.AppointmentId == parsedAppointmentId, currentUserId, currentUser.Role!.RoleName, UserRolesName.Guest.ToString(), UserRolesName.Doctor.ToString());

        if (appointment is null)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: AppointmentMessages.APPOINTMENT_NOT_FOUND
            );
        }

        // Không cho update nếu đã hoàn tất hoặc hủy
        if (appointment.StatusId == (int)AppointmentStatus.Completed ||
            appointment.StatusId == (int)AppointmentStatus.Canceled)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: AppointmentMessages.CANNOT_UPDATE_A_COMPLETED_OR_CANCELED_APPOINTMENT
            );
        }

        var errors = new Dictionary<string, string>();

        // 1. Validate StatusId
        if (!string.IsNullOrWhiteSpace(dto.StatusId))
        {
            if (!ValidateAndParseStatus.TryParseStatus<object>(dto.StatusId, out int? parsedStatusId, out var errorResponseStatus, isFromBody: true))
            {
                return errorResponseStatus!;
            }
            if (parsedStatusId.HasValue)
            {
                if (!EnumValidationService.IsValid<AppointmentStatus>(parsedStatusId.Value))
                {
                    errors["status_id"] = $"{AppointmentMessages.STATUS_ID_IS_INVALID}: {EnumValidationService.GetValidEnumValues<AppointmentStatus>()}";
                }
                else if (!ValidateStatusChange.ValidateStatusChangeForRole(currentUser.Role.RoleName, appointment.Status!.StatusId, parsedStatusId.Value))
                {
                    errors["status_id"] = AppointmentMessages.UPDATE_STATUS_ID_IS_INVALID;
                }
                else
                {
                    appointment.StatusId = parsedStatusId.Value;
                }
            }
        }

        // 2. Validate Date & Time & Doctor Id
        bool hasDoctor = !string.IsNullOrWhiteSpace(dto.DoctorId);
        bool hasDate = !string.IsNullOrWhiteSpace(dto.Date);
        bool hasTime = !string.IsNullOrWhiteSpace(dto.Time);
        DateTime parsedDate = default;
        TimeSpan parsedTime = default;
        Guid parsedDoctorId = default;

        if (hasDoctor || hasDate || hasTime)
        {

            if (appointment.StatusId == (int)AppointmentStatus.Scheduled)
            {
                // Nếu thiếu bất kỳ cái nào
                if (!hasDoctor || !hasDate || !hasTime)
                {
                    errors["scheduled"] = AppointmentMessages.MUST_HAVE_DOCTOR_ID_AND_DATE_AND_TIME_TOGETHER_WHEN_UPDATING_SCHEDULED;
                }
                else
                {
                    // Validate Date format
                    if (!DateTime.TryParseExact(dto.Date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out parsedDate))
                    {
                        errors["date"] = AppointmentMessages.DATE_IS_INVALID;
                    }

                    // Validate Time format
                    if (!TimeSpan.TryParse(dto.Time, out parsedTime))
                    {
                        errors["time"] = AppointmentMessages.TIME_IS_INVALID;
                    }

                    // Validate DoctorId
                    if (!Guid.TryParse(dto.DoctorId, out parsedDoctorId))
                    {
                        errors["doctor_id"] = AppointmentMessages.DOCTOR_ID_IS_INVALID;
                    }


                    // Nếu date và time hợp lệ thì kiểm tra thời gian hợp lệ
                    if (!errors.ContainsKey("date") && !errors.ContainsKey("time") && !errors.ContainsKey("doctor_id"))
                    {
                        var scheduledDateTime = parsedDate.Date.Add(parsedTime);
                        if (scheduledDateTime < DateTime.UtcNow)
                        {
                            errors["scheduled_date"] = AppointmentMessages.SCHEDULED_DATE_IS_INVALID;
                        }
                        else
                        {
                            // Check availability
                            var availableSlots = await GetAvailableTimeSlotsDoctorService(parsedDate.ToString(), parsedDoctorId.ToString(), false);
                            System.Console.WriteLine(parsedDate.ToString());
                            System.Console.WriteLine(parsedDoctorId.ToString());

                            if (availableSlots.Data is null)
                            {
                                return new ResponseService<object>(statusCode: availableSlots.StatusCode, message: availableSlots.Message);
                            }
                            bool slotAvailable = availableSlots.Data.Any(s => s.Time == parsedTime.ToString(@"hh\:mm") && s.IsAvailable);
                            if (!slotAvailable)
                            {
                                errors["time"] = AppointmentMessages.SELECTED_TIME_SLOT_IS_NOT_AVAILABLE;
                            }
                            else
                            {
                                appointment.DoctorId = parsedDoctorId;
                                appointment.ScheduledDate = scheduledDateTime;
                            }
                        }
                    }
                }

            }
        }

        // Note
        CheckAndUpdateString.CheckAndUpdateValueString(errors, "note", dto.Note, appointment.Note!, AppointmentMessages.NOTE_IS_REQUIRED, val => appointment.Note = val);



        // Nếu có lỗi thì return luôn
        if (errors.Count > 0)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.UnprocessableEntity,
                message: AppointmentMessages.ERROR,
                errors: errors
            );
        }

        appointment.UpdatedAt = DateTime.Now;
        await unitOfWork.SaveChangesAsync();

        return new ResponseService<object>(
            statusCode: (int)HttpStatusCode.OK,
            message: AppointmentMessages.UPDATE_APPOINTMENT_SUCCESSFULLY
        );
    }

    #endregion

    #region UPDATE APPOINTMENT STATUS
    public async Task<ResponseService<object>> UpdateAppointmentStatusService(Guid currentUserId, UpdateAppointmentStatusDto dto, string appointmentId)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }
        if (!int.TryParse(appointmentId, out var parsedAppointmentId))
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: AppointmentMessages.APPOINTMENT_MUST_BE_A_NUMBER
            );
        }
        var appointment = await appointmentRepo.GetAppointmentById(a => a.AppointmentId == parsedAppointmentId, currentUserId, currentUser.Role!.RoleName, UserRolesName.Guest.ToString(), UserRolesName.Doctor.ToString());

        if (appointment is null)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: AppointmentMessages.APPOINTMENT_NOT_FOUND
            );
        }

        // Không cho update nếu đã hoàn tất hoặc hủy
        if (appointment.StatusId == (int)AppointmentStatus.Completed ||
            appointment.StatusId == (int)AppointmentStatus.Canceled)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: AppointmentMessages.CANNOT_UPDATE_A_COMPLETED_OR_CANCELED_APPOINTMENT
            );
        }

        var errors = new Dictionary<string, string>();

        // 1. Validate StatusId
        if (!string.IsNullOrWhiteSpace(dto.StatusId))
        {
            if (!ValidateAndParseStatus.TryParseStatus<object>(dto.StatusId, out int? parsedStatusId, out var errorResponseStatus, isFromBody: true))
            {
                return errorResponseStatus!;
            }
            if (parsedStatusId.HasValue)
            {
                if (!EnumValidationService.IsValid<AppointmentStatus>(parsedStatusId.Value))
                {
                    errors["status_id"] = $"{AppointmentMessages.STATUS_ID_IS_INVALID}: {EnumValidationService.GetValidEnumValues<AppointmentStatus>()}";
                }
                else if (!ValidateStatusChange.ValidateStatusChangeForRole(currentUser.Role.RoleName, appointment.Status!.StatusId, parsedStatusId.Value))
                {
                    errors["status_id"] = AppointmentMessages.UPDATE_STATUS_ID_IS_INVALID;
                }
                else
                {
                    appointment.StatusId = parsedStatusId.Value;
                }
            }
        }

        // Note
        // CheckAndUpdateString.CheckAndUpdateValueString(errors, "note", dto.Note, appointment.Note!, AppointmentMessages.NOTE_IS_REQUIRED, val => appointment.Note = val);

        // Note
        if (appointment.StatusId == (int)AppointmentStatus.Canceled)
        {

            if (string.IsNullOrWhiteSpace(dto.Note))
            {
                errors["note"] = AppointmentMessages.NOTE_IS_REQUIRED;
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Note, @"^[A-Za-zÀ-ỹ\s]{5,100}$"))
            {
                errors["note"] = AppointmentMessages.NOTE_IS_INVALID;
            }
        }

        // Nếu có lỗi thì return luôn
        if (errors.Count > 0)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.UnprocessableEntity,
                message: AppointmentMessages.ERROR,
                errors: errors
            );
        }

        // Add vào bảng History
        var appointmentStatusHistory = new AppointmentStatusHistory
        {
            StatusId = (int)appointment.StatusId!,
            Appointment = appointment,
            Note = appointment.StatusId! switch
            {
                (int)AppointmentStatus.Scheduled => AppointmentMessages.SCHEDULED_STATUS,
                (int)AppointmentStatus.Awaiting => AppointmentMessages.AWAITING_STATUS,
                (int)AppointmentStatus.Examining => AppointmentMessages.EXAMINING_STATUS,
                (int)AppointmentStatus.Completed => AppointmentMessages.COMPLETED_STATUS,
                (int)AppointmentStatus.Canceled => dto.Note,
                (int)AppointmentStatus.AwaitingTesting => AppointmentMessages.AWAITING_STATUS,
                (int)AppointmentStatus.TestingCompleted => AppointmentMessages.TESTING_COMPLETED_STATUS,
                _ => "Không xác định được"
            }
        };
        await appointmentStatusHistoryRepo.AddAsync(appointmentStatusHistory);

        appointment.UpdatedAt = DateTime.Now;
        await unitOfWork.SaveChangesAsync();

        return new ResponseService<object>(
            statusCode: (int)HttpStatusCode.OK,
            message: AppointmentMessages.UPDATE_APPOINTMENT_SUCCESSFULLY
        );
    }

    #endregion


    #region UPDATE APPOINTMENT FOR DOCTOR AND GUEST
    public async Task<ResponseService<object>> UpdateAppointmentDagService(Guid currentUserId, UpdateAppointmentStatusDto dto, string appointmentId)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }
        if (!int.TryParse(appointmentId, out var parsedAppointmentId))
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: AppointmentMessages.APPOINTMENT_MUST_BE_A_NUMBER
            );
        }
        var appointment = await appointmentRepo.GetAppointmentById(a => a.AppointmentId == parsedAppointmentId, currentUserId, currentUser.Role!.RoleName, UserRolesName.Guest.ToString(), UserRolesName.Doctor.ToString());

        if (appointment is null)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: AppointmentMessages.APPOINTMENT_NOT_FOUND
            );
        }

        // Không cho update nếu đã hoàn tất hoặc hủy
        if (appointment.StatusId == (int)AppointmentStatus.Completed ||
            appointment.StatusId == (int)AppointmentStatus.Canceled)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: AppointmentMessages.CANNOT_UPDATE_A_COMPLETED_OR_CANCELED_APPOINTMENT
            );
        }

        var errors = new Dictionary<string, string>();
        // 1. Validate StatusId
        if (!ValidateAndParseStatus.TryParseStatus<object>(dto.StatusId, out int? parsedStatusId, out var errorResponseStatus, isFromBody: true))
        {
            return errorResponseStatus!;
        }
        if (parsedStatusId.HasValue)
        {
            if (!EnumValidationService.IsValid<AppointmentStatus>(parsedStatusId.Value))
            {
                errors["status_id"] = $"{AppointmentMessages.STATUS_ID_IS_INVALID}: {EnumValidationService.GetValidEnumValues<AppointmentStatus>()}";
            }
            else if (!ValidateStatusChange.ValidateStatusChangeForRole(currentUser.Role.RoleName, appointment.Status!.StatusId, parsedStatusId.Value))
            {
                errors["status_id"] = AppointmentMessages.UPDATE_STATUS_ID_IS_INVALID;
            }
            else
            {
                appointment.StatusId = parsedStatusId.Value;
            }
        }
        // Nếu có lỗi thì return luôn
        if (errors.Count > 0)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.UnprocessableEntity,
                message: AppointmentMessages.ERROR,
                errors: errors
            );
        }


        // Add vào bảng History
        var appointmentStatusHistory = new AppointmentStatusHistory
        {
            StatusId = (int)appointment.StatusId!,
            Appointment = appointment,
            Note = appointment.StatusId! switch
            {
                (int)AppointmentStatus.Scheduled => AppointmentMessages.SCHEDULED_STATUS,
                (int)AppointmentStatus.Awaiting => AppointmentMessages.AWAITING_STATUS,
                (int)AppointmentStatus.Examining => AppointmentMessages.EXAMINING_STATUS,
                (int)AppointmentStatus.Completed => AppointmentMessages.COMPLETED_STATUS,
                (int)AppointmentStatus.Canceled => dto.Note,
                (int)AppointmentStatus.AwaitingTesting => AppointmentMessages.AWAITING_STATUS,
                (int)AppointmentStatus.TestingCompleted => AppointmentMessages.TESTING_COMPLETED_STATUS,
                _ => "Không xác định được"
            }
        };
        await appointmentStatusHistoryRepo.AddAsync(appointmentStatusHistory);
        appointment.UpdatedAt = DateTime.Now;
        await appointmentRepo.Update(appointment);
        await unitOfWork.SaveChangesAsync();

        return new ResponseService<object>(
            statusCode: (int)HttpStatusCode.OK,
            message: AppointmentMessages.UPDATE_APPOINTMENT_SUCCESSFULLY
        );
    }
    #endregion


    #region RECEPTIONIST CHECK IN
    public async Task<ResponseService<string>> CheckInService(Guid currentUserId, string appointmentId)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }
        if (!int.TryParse(appointmentId, out var parsedAppointmentId))
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: AppointmentMessages.APPOINTMENT_MUST_BE_A_NUMBER
            );
        }
        var appointment = await appointmentRepo.GetAppointmentById(a => a.AppointmentId == parsedAppointmentId, currentUserId, currentUser.Role!.RoleName, UserRolesName.Guest.ToString(), UserRolesName.Doctor.ToString());

        if (appointment is null)
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: AppointmentMessages.APPOINTMENT_NOT_FOUND
            );
        }

        // Không cho update nếu đã hoàn tất hoặc hủy
        if (appointment.StatusId == (int)AppointmentStatus.Completed ||
            appointment.StatusId == (int)AppointmentStatus.Canceled)
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: AppointmentMessages.CANNOT_UPDATE_A_COMPLETED_OR_CANCELED_APPOINTMENT
            );
        }

        // Lấy danh sách services
        var entryService = await serviceRepo.GetEntryService();
        if (entryService is null)
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: AppointmentMessages.ENTRY_SERVICE_NOT_FOUND
            );
        }

        appointment.StatusId = (int)AppointmentStatus.Awaiting;
        appointment.UpdatedAt = DateTime.Now;

        // Add vào bảng History
        var appointmentStatusHistory = new AppointmentStatusHistory
        {
            StatusId = (int)appointment.StatusId!,
            Appointment = appointment,
            Note = AppointmentMessages.AWAITING_STATUS,
        };

        // Add vào Billing và Billing Detail
        var billing = new Billing
        {
            AppointmentId = appointment.AppointmentId,
            TotalAmount = entryService.Price,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        var billingDetail = new BillingDetail
        {
            Billing = billing,
            Service = entryService,
            PaymentStatusId = (int)PaymentStatusEnum.Unpaid,
            Price = entryService.Price
        };


        await appointmentStatusHistoryRepo.AddAsync(appointmentStatusHistory);
        await billingRepo.AddAsync(billing);
        await billingDetailRepo.AddAsync(billingDetail);
        await unitOfWork.SaveChangesAsync();

        return new ResponseService<string>(
            statusCode: (int)HttpStatusCode.OK,
            message: AppointmentMessages.CHECK_IN_SUCCESSFULLY
        );
    }
    #endregion

    #region GET APPOINTMENT HISTORY     
    public async Task<ResponseService<List<AppointmentStatusHistoryDto>>> GetAppointmentHistoriesService(Guid currentUserId, string appointmentId)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
        {
            return new ResponseService<List<AppointmentStatusHistoryDto>>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }
        if (!int.TryParse(appointmentId, out var parsedAppointmentId))
        {
            return new ResponseService<List<AppointmentStatusHistoryDto>>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: AppointmentMessages.APPOINTMENT_MUST_BE_A_NUMBER
            );
        }
        var appointment = await appointmentRepo.GetAppointmentById(a => a.AppointmentId == parsedAppointmentId, currentUserId, currentUser.Role!.RoleName, UserRolesName.Guest.ToString(), UserRolesName.Doctor.ToString());

        if (appointment is null)
        {
            return new ResponseService<List<AppointmentStatusHistoryDto>>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: AppointmentMessages.APPOINTMENT_NOT_FOUND
            );
        }
        var appointmentStatusHistories = await appointmentStatusHistoryRepo.GetAppointmentStatusHistories(parsedAppointmentId);
        var appointmentStatusHistoriesMapper = _mapper.Map<List<AppointmentStatusHistoryDto>>(appointmentStatusHistories);
        return new ResponseService<List<AppointmentStatusHistoryDto>>(
            statusCode: (int)HttpStatusCode.OK,
            message: StatisticalMessages.GET_APPOINTMENT_HISTORIES_SUCCESSFULLY,
            data: appointmentStatusHistoriesMapper
        );
    }
    #endregion



}