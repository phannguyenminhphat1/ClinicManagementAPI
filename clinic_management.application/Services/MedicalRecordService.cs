using System.Net;
using System.Text.Json;
using AutoMapper;
using clinic_management.infrastructure.Models;
using CloudinaryDotNet.Actions;

public interface IMedicalRecordService
{
    public Task<ResponseService<object>> ExaminingService(ExaminingDto dto, Guid currentUserId, string appointmentId);
    public Task<ResponseService<ResponsePagedService<List<MedicalRecordDetailSummaryDto>>>> GetAllMedicalRecordDetailService(Guid currentUserId, PaginationDto paginationDto);
    public Task<ResponseService<List<GetServiceDto>>> GetAllServicesTestService();

    public Task<ResponseService<GetMedicalRecordDetailDto>> GetMedicalRecordDetailByAppointmentIdService(Guid currentUserId, string appointmentId);
    public Task<ResponseService<object>> AddNewServicesInMedicalRecordService(Guid currentUserId, AddNewServicesDto dto);

    public Task<ResponseService<string>> UpdateExaminingService(Guid currentUserId, UpdateExaminingDto dto, string appointmentId);
    public Task<ResponseService<GetMedicalRecordDto>> GetMedicalRecordByIdService(Guid currentUserId, string medicalRecordId);
    public Task<ResponseService<ResponsePagedService<List<GetMedicalRecordDetailDto>>>> GetMedicalRecordsDetailByMedicalRecordIdService(Guid currentUserId, string medicalRecordId, PaginationDto paginationDto);

}


public class MedicalRecordService(IMapper _mapper, IMedicalRecordRepository medicalRecordRepo, IAppointmentRepository appointmentRepo, IUnitOfWork unitOfWork, IUserRepository userRepo, IServiceRepository serviceRepo, IMedicalTestRepository medicalTestRepo, IMedicalRecordDetailRepository medicalRecordDetailRepo, IMedicalRecordSummaryRepository medicalRecordSummaryRepo, IBillingRepository billingRepo, IBillingDetailRepository billingDetailRepo, IAppointmentStatusHistoryRepository appointmentStatusHistoryRepo) : IMedicalRecordService
{
    #region VALIDATE EXAMINING DATA
    private Dictionary<string, string> ValidateExaminingData(ExaminingDto dto)
    {
        var errors = new Dictionary<string, string>();

        if (!dto.RequiresTest.HasValue)
            errors["requires_test"] = MedicalRecordMessages.REQUIRES_TEST_IS_REQUIRED;

        if (string.IsNullOrWhiteSpace(dto.Symptoms))
            errors["symptoms"] = MedicalRecordMessages.SYMPTOMS_IS_REQUIRED;

        if (dto.RequiresTest == false && string.IsNullOrWhiteSpace(dto.Diagnosis))
            errors["diagnosis"] = MedicalRecordMessages.DIAGNOSIS_IS_REQUIRED_WHEN_NO_TEST;

        if (dto.RequiresTest == true && (dto.ServiceIds == null || dto.ServiceIds.Count == 0))
            errors["service_ids"] = MedicalRecordMessages.SERVICE_ID_IS_REQUIRED;

        return errors;
    }
    #endregion


    #region VALIDATE USER + APPOINTMENT
    private async Task<(ResponseService<T>? Response, (User user, Appointment appointment)? Result)>
    ValidateUserAndAppointment<T>(Guid currentUserId, string appointmentId)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
            return (new ResponseService<T>((int)HttpStatusCode.NotFound, UserMessages.CURRENT_USER_NOT_FOUND), null);

        if (!int.TryParse(appointmentId, out var parsedId))
            return (new ResponseService<T>((int)HttpStatusCode.BadRequest, AppointmentMessages.APPOINTMENT_MUST_BE_A_NUMBER), null);

        var appointment = await appointmentRepo.GetAppointmentById(
            a => a.AppointmentId == parsedId,
            currentUserId,
            currentUser.Role!.RoleName,
            UserRolesName.Guest.ToString(),
            UserRolesName.Doctor.ToString()
        );

        if (appointment == null)
            return (new ResponseService<T>((int)HttpStatusCode.NotFound, AppointmentMessages.APPOINTMENT_NOT_FOUND), null);

        if (appointment.StatusId != (int)AppointmentStatus.Examining && appointment.StatusId != (int)AppointmentStatus.AwaitingTesting && appointment.StatusId != (int)AppointmentStatus.TestingCompleted)
            return (new ResponseService<T>((int)HttpStatusCode.BadRequest, AppointmentMessages.APPOINTMENT_NOT_EXAMINING), null);

        return (null, (currentUser, appointment));
    }
    #endregion

    #region CREATE MEDICAL RECORD
    private async Task<MedicalRecord> GetOrCreateMedicalRecord(Guid patientId)
    {
        var record = await medicalRecordRepo.GetMedicalRecordByPatient(patientId);
        if (record != null) return record;

        record = new MedicalRecord
        {
            PatientId = patientId,
            CreatedAt = DateTime.UtcNow
        };
        await medicalRecordRepo.AddAsync(record);
        return record;
    }
    #endregion


    #region CREATE MEDICAL RECORD DETAIL
    private async Task<MedicalRecordDetail> CreateMedicalRecordDetail(
    ExaminingDto dto, Appointment appointment, MedicalRecord record)
    {
        var detail = new MedicalRecordDetail
        {
            MedicalRecord = record,
            Appointment = appointment,
            Symptoms = dto.Symptoms!,
            Diagnosis = dto.RequiresTest!.Value ? dto.Diagnosis ?? "" : dto.Diagnosis!,
            Notes = dto.Notes,
            RequiresTest = dto.RequiresTest!.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await medicalRecordDetailRepo.AddAsync(detail);
        return detail;
    }
    #endregion

    #region HANDLE MEDICAL TEST IF REQUIRES TEST TRUE OR FALSE
    private async Task<ResponseService<object>?> HandleMedicalTests(
    List<string>? serviceIds, Appointment appointment, MedicalRecordDetail detail)
    {
        var billing = await billingRepo.GetBillingByAppointmentId(appointment.AppointmentId);
        if (billing == null)
        {
            return new ResponseService<object>((int)HttpStatusCode.NotFound, BillingMessages.BILLING_NOT_FOUND);
        }

        foreach (var serviceIdStr in serviceIds!)
        {
            if (!int.TryParse(serviceIdStr, out var parsedId))
            {
                return new ResponseService<object>((int)HttpStatusCode.BadRequest, $"Mã dịch vụ: '{serviceIdStr}' phải là số!");
            }

            var service = await serviceRepo.SingleOrDefaultAsync(s => s.ServiceId == parsedId);
            if (service == null)
            {
                return new ResponseService<object>((int)HttpStatusCode.BadRequest, $"Mã dịch vụ '{parsedId}' không tìm thấy!");
            }
            var duplicate = await medicalTestRepo.SingleOrDefaultAsync(
                t => t.MedicalRecordDetailId == detail.MedicalRecordDetailId && t.ServiceId == parsedId);

            if (duplicate != null)
            {
                return new ResponseService<object>((int)HttpStatusCode.BadRequest,
                    $"{MedicalRecordMessages.MEDICAL_TEST_SERVICE_IS_ALREADY_EXIST} - Tên dịch vụ: {service.ServiceName}");
            }
            await medicalTestRepo.AddAsync(new MedicalTest
            {
                MedicalRecordDetail = detail,
                ServiceId = service.ServiceId,
                StatusId = (int)MedicalTestStatusEnum.AwaitingTesting,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await billingDetailRepo.AddAsync(new BillingDetail
            {
                Billing = billing,
                ServiceId = service.ServiceId,
                PaymentStatusId = (int)PaymentStatusEnum.Unpaid,
                Price = service.Price
            });

            billing.TotalAmount += service.Price;
        }

        billing.UpdatedAt = DateTime.UtcNow;
        await billingRepo.Update(billing);

        await appointmentStatusHistoryRepo.AddAsync(new AppointmentStatusHistory
        {
            StatusId = (int)AppointmentStatus.AwaitingTesting,
            Appointment = appointment,
            Note = AppointmentMessages.AWAITING_TESTING_STATUS
        });

        return null;
    }
    #endregion

    #region UPDATE APPOINTMENT STATUS HISTORY
    private async Task UpdateAppointmentStatus(Appointment appointment, bool requiresTest)
    {
        appointment.StatusId = requiresTest
            ? (int)AppointmentStatus.AwaitingTesting
            : (int)AppointmentStatus.Completed;

        await appointmentStatusHistoryRepo.AddAsync(new AppointmentStatusHistory
        {
            StatusId = appointment.StatusId.Value,
            Appointment = appointment,
            Note = requiresTest
                ? AppointmentMessages.AWAITING_TESTING_STATUS
                : AppointmentMessages.COMPLETED_STATUS
        });


        appointment.UpdatedAt = DateTime.UtcNow;
        await appointmentRepo.Update(appointment);
    }


    #endregion


    #region EXAMINING
    public async Task<ResponseService<object>> ExaminingService(ExaminingDto dto, Guid currentUserId, string appointmentId)
    {
        // 1. Kiểm tra người dùng và cuộc hẹn
        var validateUserAndAppointmentResult = await ValidateUserAndAppointment<object>(currentUserId, appointmentId);
        if (validateUserAndAppointmentResult.Response != null)
            return validateUserAndAppointmentResult.Response;

        var (currentUser, appointment) = validateUserAndAppointmentResult.Result!.Value;

        // 2. Validate input (triệu chứng, chẩn đoán, dịch vụ, ...)
        var validationErrors = ValidateExaminingData(dto);
        if (validationErrors.Count > 0)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.UnprocessableEntity,
                message: AppointmentMessages.ERROR,
                errors: validationErrors
            );
        }

        // 3. Lấy hoặc tạo hồ sơ bệnh án
        var medicalRecord = await GetOrCreateMedicalRecord(appointment.Patient!.UserId);

        // 4. Tạo chi tiết hồ sơ bệnh án
        var detail = await CreateMedicalRecordDetail(dto, appointment, medicalRecord);

        // 5. Nếu có chỉ định xét nghiệm thì tạo MedicalTest
        if (dto.RequiresTest == true)
        {
            var result = await HandleMedicalTests(dto.ServiceIds, appointment, detail);
            if (result != null)
            {
                return result;
            }
        }

        // 6. Cập nhật trạng thái cuộc hẹn & lưu lịch sử
        await UpdateAppointmentStatus(appointment, dto.RequiresTest!.Value);

        await unitOfWork.SaveChangesAsync();

        return new ResponseService<object>(
            statusCode: (int)HttpStatusCode.OK,
            message: MedicalRecordMessages.DIAGNOSE_SUCCESSFULLY
        );
    }
    #endregion

    #region GET MEDICAL RECORD DETAILT BY APPOINTMENT ID

    public async Task<ResponseService<GetMedicalRecordDetailDto>> GetMedicalRecordDetailByAppointmentIdService(Guid currentUserId, string appointmentId)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
            return new ResponseService<GetMedicalRecordDetailDto>((int)HttpStatusCode.NotFound, UserMessages.CURRENT_USER_NOT_FOUND);

        if (!int.TryParse(appointmentId, out var parsedId))
            return new ResponseService<GetMedicalRecordDetailDto>((int)HttpStatusCode.BadRequest, AppointmentMessages.APPOINTMENT_MUST_BE_A_NUMBER);

        var appointment = await appointmentRepo.GetAppointmentById(
            a => a.AppointmentId == parsedId,
            currentUserId,
            currentUser.Role!.RoleName,
            UserRolesName.Guest.ToString(),
            UserRolesName.Doctor.ToString()
        );

        if (appointment == null)
            return new ResponseService<GetMedicalRecordDetailDto>((int)HttpStatusCode.NotFound, AppointmentMessages.APPOINTMENT_NOT_FOUND);

        var result = await medicalRecordDetailRepo.GetMedicalRecordDetail(m => m.AppointmentId == appointment.AppointmentId, currentUserId, currentUser.Role!.RoleName, UserRolesName.Guest.ToString(), UserRolesName.Doctor.ToString());
        if (result == null)
        {
            return new ResponseService<GetMedicalRecordDetailDto>(
             statusCode: (int)HttpStatusCode.BadRequest,
             message: MedicalRecordMessages.THIS_APPOINTMENT_DO_NOT_HAVE_MEDICAL_RECORD_DETAIL
         );

        }
        var medicalRecordDetailMapper = _mapper.Map<GetMedicalRecordDetailDto>(result);
        return new ResponseService<GetMedicalRecordDetailDto>(
            statusCode: (int)HttpStatusCode.OK,
            message: MedicalRecordMessages.GET_MEDICAL_RECORD_DETAIL_SUCCESSFULLY,
            data: medicalRecordDetailMapper
        );

    }
    #endregion


    #region UPDATE EXAMINING
    public async Task<ResponseService<string>> UpdateExaminingService(Guid currentUserId, UpdateExaminingDto dto, string appointmentId)
    {
        // 1. Kiểm tra người dùng và cuộc hẹn
        var validateUserAndAppointmentResult = await ValidateUserAndAppointment<string>(currentUserId, appointmentId);
        if (validateUserAndAppointmentResult.Response != null)
            return validateUserAndAppointmentResult.Response;

        var (currentUser, appointment) = validateUserAndAppointmentResult.Result!.Value;

        // 2. Lấy medical record detail
        var medicalRecordDetail = await medicalRecordDetailRepo.GetMedicalRecordDetailWithAppointmentAsync(mrd => mrd.AppointmentId == appointment.AppointmentId);
        if (medicalRecordDetail == null)
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: MedicalRecordMessages.MEDICAL_RECORD_DETAIL_NOT_FOUND
            );
        }
        medicalRecordDetail.Symptoms = dto.Symptoms!;
        medicalRecordDetail.Diagnosis = dto.Diagnosis!;
        medicalRecordDetail.Notes = dto.Notes;

        appointment.StatusId = (int)AppointmentStatus.Completed;

        await appointmentStatusHistoryRepo.AddAsync(new AppointmentStatusHistory
        {
            StatusId = appointment.StatusId.Value,
            Appointment = appointment,
            Note = AppointmentMessages.COMPLETED_STATUS
        });
        appointment.UpdatedAt = DateTime.UtcNow;

        await appointmentRepo.Update(appointment);
        await medicalRecordDetailRepo.Update(medicalRecordDetail);
        await unitOfWork.SaveChangesAsync();
        return new ResponseService<string>(
            statusCode: (int)HttpStatusCode.OK,
            message: MedicalRecordMessages.UPDATE_AND_COMPLETE_MEDICAL_RECORD_DETAIL_SUCCESSFULLY
        );
    }
    #endregion

    #region GET ALL MEDICAL RECORD DETAIL
    public async Task<ResponseService<ResponsePagedService<List<MedicalRecordDetailSummaryDto>>>> GetAllMedicalRecordDetailService(Guid currentUserId, PaginationDto paginationDto)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
        {
            return new ResponseService<ResponsePagedService<List<MedicalRecordDetailSummaryDto>>>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }
        if (!ValidateAndParsePagination.TryParsePagination<ResponsePagedService<List<MedicalRecordDetailSummaryDto>>>(paginationDto.Page, paginationDto.PageSize, out int? pageParsed, out int? pageSizeParsed, out var errorResponsePaged))
        {
            return errorResponsePaged!;
        }
        int currentPage = pageParsed ?? 1;
        int pageSize = pageSizeParsed ?? 10;
        var (result, totalRecords) = await medicalRecordSummaryRepo.GetAllMedicalRecordDetail(mrds => mrds.PatientId == currentUserId, currentPage, pageSize);
        int totalPage = (int)Math.Ceiling((double)totalRecords / pageSize);
        var resultMapper = _mapper.Map<List<MedicalRecordDetailSummaryDto>>(result);
        var resultResponse = new ResponsePagedService<List<MedicalRecordDetailSummaryDto>>(
            data: resultMapper,
            currentPage: currentPage,
            pageSize: pageSize,
            totalPage: totalPage,
            totalItem: totalRecords
        );
        return new ResponseService<ResponsePagedService<List<MedicalRecordDetailSummaryDto>>>(
            statusCode: (int)HttpStatusCode.OK,
            message: MedicalRecordMessages.GET_ALL_MEDICAL_RECORD_DETAIL_SUMMARY_SUCCESSFULLY,
            data: resultResponse
        );
    }
    #endregion

    #region GET ALL SERVICES
    public async Task<ResponseService<List<GetServiceDto>>> GetAllServicesTestService()
    {
        var servicesTest = await serviceRepo.GetAllServices();
        var servicesTestMapper = _mapper.Map<List<GetServiceDto>>(servicesTest);
        return new ResponseService<List<GetServiceDto>>(
            statusCode: (int)HttpStatusCode.OK,
            message: MedicalRecordMessages.DIAGNOSE_SUCCESSFULLY,
            data: servicesTestMapper
        );
    }
    #endregion


    #region ADD NEW SERVICES IN MEDICAL RECORD DETAIL

    public async Task<ResponseService<object>> AddNewServicesInMedicalRecordService(Guid currentUserId, AddNewServicesDto dto)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }
        if (!int.TryParse(dto.MedicalRecordDetailId, out var medicalRecordDetailId))
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: MedicalRecordMessages.MEDICAL_RECORD_DETAIL_MUST_BE_A_NUMBER
            );
        }
        var medicalRecordDetail = await medicalRecordDetailRepo.GetMedicalRecordDetail(x => x.MedicalRecordDetailId == medicalRecordDetailId, currentUserId, currentUser.Role!.RoleName, UserRolesName.Guest.ToString(), UserRolesName.Doctor.ToString());

        if (medicalRecordDetail == null)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: MedicalRecordMessages.MEDICAL_RECORD_DETAIL_NOT_FOUND
            );
        }
        var result = await HandleMedicalTests(dto.ServiceIds, medicalRecordDetail.Appointment!, medicalRecordDetail);
        if (result != null)
        {
            return result;
        }
        await unitOfWork.SaveChangesAsync();
        return new ResponseService<object>(
            statusCode: (int)HttpStatusCode.OK,
            message: MedicalRecordMessages.ADD_NEW_SERVICES_IN_MEDICAL_RECORD_DETAIL_SUCCESSFULLY
        );
    }
    #endregion


    #region GET MEDICAL RECORD BY ID
    public async Task<ResponseService<GetMedicalRecordDto>> GetMedicalRecordByIdService(Guid currentUserId, string medicalRecordId)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
        {
            return new ResponseService<GetMedicalRecordDto>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.USER_NOT_FOUND
            );
        }
        if (!int.TryParse(medicalRecordId, out var parsedMedicalRecordId))
        {
            return new ResponseService<GetMedicalRecordDto>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: MedicalRecordMessages.MEDICAL_RECORD_ID_MUST_BE_A_NUMBER
            );
        }
        var medicalRecord = await medicalRecordRepo.GetMedicalRecordById(parsedMedicalRecordId, currentUserId, currentUser.Role!.RoleName, UserRolesName.Guest.ToString(), UserRolesName.Doctor.ToString());
        if (medicalRecord == null)
        {
            return new ResponseService<GetMedicalRecordDto>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: MedicalRecordMessages.MEDICAL_RECORD_NOT_FOUND
            );
        }

        var medicalRecordMapper = _mapper.Map<GetMedicalRecordDto>(medicalRecord);
        return new ResponseService<GetMedicalRecordDto>(
            statusCode: (int)HttpStatusCode.OK,
            message: MedicalRecordMessages.GET_MEDICAL_RECORD_SUCCESSFULLY,
            data: medicalRecordMapper
        );
    }
    #endregion

    #region GET ALL MEDICAL RECORDS DETAIL BY MEDICAL RECORD ID
    public async Task<ResponseService<ResponsePagedService<List<GetMedicalRecordDetailDto>>>> GetMedicalRecordsDetailByMedicalRecordIdService(Guid currentUserId, string medicalRecordId, PaginationDto paginationDto)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
        {
            return new ResponseService<ResponsePagedService<List<GetMedicalRecordDetailDto>>>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }
        if (!ValidateAndParsePagination.TryParsePagination<ResponsePagedService<List<GetMedicalRecordDetailDto>>>(paginationDto.Page, paginationDto.PageSize, out int? pageParsed, out int? pageSizeParsed, out var errorResponsePaged))
        {
            return errorResponsePaged!;
        }
        if (!int.TryParse(medicalRecordId, out var parsedMedicalRecordId))
        {
            return new ResponseService<ResponsePagedService<List<GetMedicalRecordDetailDto>>>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: MedicalRecordMessages.MEDICAL_RECORD_ID_MUST_BE_A_NUMBER
            );
        }
        var medicalRecord = await medicalRecordRepo.GetMedicalRecordById(parsedMedicalRecordId, currentUserId, currentUser.Role!.RoleName, UserRolesName.Guest.ToString(), UserRolesName.Doctor.ToString());
        if (medicalRecord == null)
        {
            return new ResponseService<ResponsePagedService<List<GetMedicalRecordDetailDto>>>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: MedicalRecordMessages.MEDICAL_RECORD_NOT_FOUND
            );
        }
        Console.WriteLine(JsonSerializer.Serialize(medicalRecord.MedicalRecordId));
        int currentPage = pageParsed ?? 1;
        int pageSize = pageSizeParsed ?? 10;
        var (result, totalRecords) = await medicalRecordDetailRepo.GetAllMedicalRecordDetail(mrds => mrds.MedicalRecordId == medicalRecord.MedicalRecordId, currentUserId, currentUser.Role!.RoleName, UserRolesName.Guest.ToString(), UserRolesName.Doctor.ToString(), currentPage, pageSize, (int)AppointmentStatus.Completed);
        int totalPage = (int)Math.Ceiling((double)totalRecords / pageSize);
        var resultMapper = _mapper.Map<List<GetMedicalRecordDetailDto>>(result);
        var resultResponse = new ResponsePagedService<List<GetMedicalRecordDetailDto>>(
            data: resultMapper,
            currentPage: currentPage,
            pageSize: pageSize,
            totalPage: totalPage,
            totalItem: totalRecords
        );
        return new ResponseService<ResponsePagedService<List<GetMedicalRecordDetailDto>>>(
            statusCode: (int)HttpStatusCode.OK,
            message: MedicalRecordMessages.GET_ALL_MEDICAL_RECORD_DETAIL_SUCCESSFULLY,
            data: resultResponse
        );
    }
    #endregion

}

