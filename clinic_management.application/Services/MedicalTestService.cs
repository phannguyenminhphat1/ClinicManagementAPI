using System.Linq.Expressions;
using System.Net;
using System.Text.Json;
using AutoMapper;
using clinic_management.infrastructure.Models;

public interface IMedicalTestService
{
    public Task<ResponseService<ResponsePagedService<List<GetMedicalTestDto>>>> GetMedicalTestsService(PaginationDto paginationDto, GetMedicalTestFilterDto dto);
    public Task<ResponseService<string>> SaveMedicalTestResultService(List<SaveMedicalTestResultDto> dtos);
    public Task<ResponseService<List<SaveMedicalTestResultDto>>> GetMedicalTestsResultByIdService(string medicalTestResultId);
    public Task<ResponseService<string>> DeleteMedicalTestResultService(string medicalTestResultId);
    public Task<ResponseService<string>> CompleteMedicalTestService(Guid currentUserId, string medicalTestId);


}

public class MedicalTestService(IUnitOfWork unitOfWork, IMapper _mapper, IMedicalTestRepository medicalTestRepo, IMedicalTestResultRepository medicalTestResultRepo, IAppointmentRepository appointmentRepo, IUserRepository userRepo, IAppointmentStatusHistoryRepository appointmentStatusHistoryRepo) : IMedicalTestService
{

    #region GET ALL MEDICAL TESTS
    public async Task<ResponseService<ResponsePagedService<List<GetMedicalTestDto>>>> GetMedicalTestsService(PaginationDto paginationDto, GetMedicalTestFilterDto dto)
    {
        int? parsedStatusId = null;
        Expression<Func<MedicalTest, bool>>? predicate = null;
        DateTime? date = null;

        // Nếu có truyền date
        if (!string.IsNullOrWhiteSpace(dto.Date))
        {
            if (!DateTime.TryParseExact(dto.Date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
            {
                return new ResponseService<ResponsePagedService<List<GetMedicalTestDto>>>(
                    statusCode: (int)HttpStatusCode.BadRequest,
                    message: AppointmentMessages.FILTER_DATE_IS_INVALID
                );
            }
            date = parsedDate;
        }

        // Validate StatusId
        if (!string.IsNullOrWhiteSpace(dto.StatusId))
        {
            if (!ValidateAndParseStatus.TryParseStatus<ResponsePagedService<List<GetMedicalTestDto>>>(dto.StatusId, out parsedStatusId, out var errorResponseStatus))
            {
                return errorResponseStatus!;
            }
            if (parsedStatusId.HasValue)
            {
                if (!EnumValidationService.IsValid<AppointmentStatus>(parsedStatusId.Value))
                {
                    return new ResponseService<ResponsePagedService<List<GetMedicalTestDto>>>(
                        statusCode: (int)HttpStatusCode.OK,
                        message: $"{AppointmentMessages.STATUS_ID_IS_INVALID}: {EnumValidationService.GetValidEnumValues<AppointmentStatus>()}"
                    );
                }
            }
        }

        if (!ValidateAndParsePagination.TryParsePagination<ResponsePagedService<List<GetMedicalTestDto>>>(paginationDto.Page, paginationDto.PageSize, out int? pageParsed, out int? pageSizeParsed, out var errorResponsePaged))
        {
            return errorResponsePaged!;
        }

        int currentPage = pageParsed ?? 1;
        int pageSize = pageSizeParsed ?? 10;

        // query
        var (medicalTests, totalRecords) = await medicalTestRepo.GetAllMedicalTests(predicate, currentPage, pageSize, date, parsedStatusId, dto.Keyword);

        int totalPage = (int)Math.Ceiling((double)totalRecords / pageSize);

        var medicalTestsMapper = _mapper.Map<List<GetMedicalTestDto>>(medicalTests);

        var resultResponse = new ResponsePagedService<List<GetMedicalTestDto>>(
            data: medicalTestsMapper,
            currentPage: currentPage,
            pageSize: pageSize,
            totalPage: totalPage,
            totalItem: totalRecords
        );

        return new ResponseService<ResponsePagedService<List<GetMedicalTestDto>>>(
            statusCode: (int)HttpStatusCode.OK,
            message: MedicalTestMessages.GET_ALL_MEDICAL_TEST_SUCCESSFULLY,
            data: resultResponse
        );
    }
    #endregion


    #region SAVE (UPDATE OR CREATE) MEDICAL TEST RESULT
    public async Task<ResponseService<string>> SaveMedicalTestResultService(List<SaveMedicalTestResultDto> dtos)
    {
        if (dtos == null || dtos.Count == 0)
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: MedicalTestMessages.NO_TEST_RESULT_PROVIDED
            );
        }

        if (!int.TryParse(dtos[0].MedicalTestId, out var parsedMedicalTestId))
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: MedicalTestMessages.MEDICAL_TEST_ID_MUST_BE_A_NUMBER
            );
        }

        var medicalTest = await medicalTestRepo.GetMedicalTestById(parsedMedicalTestId);
        if (medicalTest == null)
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: MedicalTestMessages.MEDICAL_TEST_NOT_FOUND
            );
        }
        if (medicalTest.StatusId == (int)MedicalTestStatusEnum.TestingCompleted)
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: MedicalTestMessages.CANNOT_CHANGE_MEDICAL_TEST_WHEN_IT_COMPLETED
            );
        }
        foreach (var item in dtos)
        {
            // Nếu có ID → update
            if (item.MedicalTestResultId.HasValue)
            {
                var existingResult = await medicalTestResultRepo.GetMedicalTestResultByMedicalTestResultId(item.MedicalTestResultId.Value);
                if (existingResult != null)
                {
                    existingResult.Parameter = item.Parameter;
                    existingResult.Unit = item.Unit;
                    existingResult.ReferenceRange = item.ReferenceRange;
                    existingResult.Value = item.Value;
                    existingResult.Note = item.Note;
                    existingResult.Image = string.IsNullOrEmpty(item.Image) ? null : item.Image;
                    existingResult.UpdatedAt = DateTime.Now;
                    await medicalTestResultRepo.Update(existingResult);
                    continue;
                }
            }

            // Nếu không có ID → thêm mới
            var newResult = new MedicalTestResult
            {
                MedicalTest = medicalTest,
                Parameter = item.Parameter,
                Unit = item.Unit,
                ReferenceRange = item.ReferenceRange,
                Value = item.Value,
                Note = string.IsNullOrEmpty(item.Note) ? null : item.Note,
                Image = string.IsNullOrEmpty(item.Image) ? null : item.Image,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await medicalTestResultRepo.AddAsync(newResult);
        }
        await unitOfWork.SaveChangesAsync();

        return new ResponseService<string>(
            statusCode: (int)HttpStatusCode.OK,
            message: MedicalTestMessages.SAVE_MEDICAL_TEST_SUCCESSFULLY
        );
    }

    #endregion

    #region CHECK IS COMPLETED ALL TESTS
    public async Task<bool> CheckIsCompletedAllTests(MedicalTest medicalTest)
    {

        var allTests = await medicalTestRepo.GetAllMedicalTestsByMedicalRecordDetailId(medicalTest.MedicalRecordDetailId!.Value);
        var isAllCompleted = allTests.All(t =>
            t.MedicalTestId == medicalTest.MedicalTestId
                ? medicalTest.StatusId == (int)MedicalTestStatusEnum.TestingCompleted
                : t.StatusId == (int)MedicalTestStatusEnum.TestingCompleted
        );
        return isAllCompleted;

    }
    #endregion


    #region GET MEDICAL TESTS RESULT BY MEDICAL TEST ID
    public async Task<ResponseService<List<SaveMedicalTestResultDto>>> GetMedicalTestsResultByIdService(string medicalTestResultId)
    {
        if (!int.TryParse(medicalTestResultId, out var parsedMedicalTestResultId))
        {
            return new ResponseService<List<SaveMedicalTestResultDto>>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: MedicalTestMessages.MEDICAL_TEST_RESULT_ID_MUST_BE_A_NUMBER
            );
        }

        var medicalTestsResult = await medicalTestResultRepo.GetMedicalTestsResultById(parsedMedicalTestResultId);
        if (medicalTestsResult == null)
        {
            return new ResponseService<List<SaveMedicalTestResultDto>>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: MedicalTestMessages.MEDICAL_TEST_RESULT_NOT_FOUND
            );
        }
        var medicalTestsResultMapper = _mapper.Map<List<SaveMedicalTestResultDto>>(medicalTestsResult);
        return new ResponseService<List<SaveMedicalTestResultDto>>(
            statusCode: (int)HttpStatusCode.OK,
            message: MedicalTestMessages.GET_MEDICAL_TEST_RESULT_SUCCESSFULLY,
            data: medicalTestsResultMapper
        );
    }
    #endregion

    #region DELETE MEDICAL TEST RESULT BY MEDICAL TEST ID
    public async Task<ResponseService<string>> DeleteMedicalTestResultService(string medicalTestResultId)
    {
        if (!int.TryParse(medicalTestResultId, out var parsedMedicalTestResultId))
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: MedicalTestMessages.MEDICAL_TEST_RESULT_ID_MUST_BE_A_NUMBER
            );
        }

        var medicalTestResult = await medicalTestResultRepo.GetMedicalTestResultByMedicalTestResultId(parsedMedicalTestResultId);
        if (medicalTestResult == null)
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: MedicalTestMessages.MEDICAL_TEST_RESULT_NOT_FOUND
            );
        }
        await medicalTestResultRepo.DeleteAsync(medicalTestResult.MedicalTestResultId);
        await unitOfWork.SaveChangesAsync();

        return new ResponseService<string>(
           statusCode: (int)HttpStatusCode.OK,
           message: MedicalTestMessages.DELETE_MEDICAL_TEST_RESULT_SUCCESSFULLY
       );
    }

    #endregion


    #region COMPLETE MEDICAL TEST BY MEDICAL TEST ID
    public async Task<ResponseService<string>> CompleteMedicalTestService(Guid currentUserId, string medicalTestId)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }

        if (!int.TryParse(medicalTestId, out var parsedMedicalTestId))
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: MedicalTestMessages.MEDICAL_TEST_ID_MUST_BE_A_NUMBER
            );
        }

        var medicalTest = await medicalTestRepo.GetMedicalTestById(parsedMedicalTestId);
        if (medicalTest == null)
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: MedicalTestMessages.MEDICAL_TEST_NOT_FOUND
            );
        }
        if (medicalTest.StatusId == (int)MedicalTestStatusEnum.TestingCompleted)
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: MedicalTestMessages.CANNOT_CHANGE_MEDICAL_TEST_WHEN_IT_COMPLETED
            );
        }
        medicalTest.StatusId = (int)MedicalTestStatusEnum.TestingCompleted;
        await medicalTestRepo.Update(medicalTest);

        bool isAllCompleted = await CheckIsCompletedAllTests(medicalTest);

        if (isAllCompleted)
        {
            var appointment = await appointmentRepo.GetAppointmentById(a => a.AppointmentId == medicalTest.MedicalRecordDetail!.AppointmentId, currentUserId, currentUser.Role!.RoleName, UserRolesName.Guest.ToString(), UserRolesName.Doctor.ToString());
            if (appointment == null)
            {
                return new ResponseService<string>(
                    statusCode: (int)HttpStatusCode.NotFound,
                    message: AppointmentMessages.APPOINTMENT_NOT_FOUND
                );
            }
            appointment.StatusId = (int)AppointmentStatus.TestingCompleted;
            var appointmentStatusHistory = new AppointmentStatusHistory
            {
                StatusId = (int)AppointmentStatus.Scheduled,
                Appointment = appointment,
                Note = AppointmentMessages.MAKE_APPOINTMENT_SUCCESSFULLY
            };
            await appointmentStatusHistoryRepo.AddAsync(appointmentStatusHistory);
            await appointmentRepo.Update(appointment);
        }
        await unitOfWork.SaveChangesAsync();


        return new ResponseService<string>(
            statusCode: (int)HttpStatusCode.OK,
            message: isAllCompleted
                ? MedicalTestMessages.ALL_TESTS_COMPLETED_AND_APPOINTMENT_UPDATED
                : MedicalTestMessages.TESTING_COMPLETED
        );
    }

    #endregion
}