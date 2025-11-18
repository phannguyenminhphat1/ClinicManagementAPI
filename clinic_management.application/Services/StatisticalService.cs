using System.Data;
using System.Net;
using System.Text.Json;
using AutoMapper;
using Microsoft.Data.SqlClient;

public interface IStatisticalService
{
    public Task<ResponseService<AppointmentStatisticalDto>> GetAppointmentStatisticService(Guid currentUserId, GetAppointmentStatisticDto dto);
}

public class StatisticalService(IAppointmentRepository appointmentRepo, IUserRepository userRepo) : IStatisticalService
{

    #region GET STATISTICAL APPOINTMENT      
    public async Task<ResponseService<AppointmentStatisticalDto>> GetAppointmentStatisticService(Guid currentUserId, GetAppointmentStatisticDto dto)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
        {
            return new ResponseService<AppointmentStatisticalDto>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }
        int groupType = (int)StatisticGroupType.Week;
        int offset = 0;
        if (!string.IsNullOrWhiteSpace(dto.Type))
        {
            if (!int.TryParse(dto.Type, out var parsedType))
            {
                return new ResponseService<AppointmentStatisticalDto>(
                    statusCode: (int)HttpStatusCode.BadRequest,
                    message: StatisticalMessages.TYPE_MUST_BE_A_NUMBER
                );
            }
            if (!EnumValidationService.IsValid<StatisticGroupType>(parsedType))
            {
                return new ResponseService<AppointmentStatisticalDto>(
                    statusCode: (int)HttpStatusCode.BadRequest,
                    message: $"{StatisticalMessages.TYPE_IS_INVALID}: {EnumValidationService.GetValidEnumValues<StatisticGroupType>()}"
                );
            }
            else
            {
                groupType = parsedType;
            }
        }
        if (!string.IsNullOrWhiteSpace(dto.Offset))
        {
            if (!int.TryParse(dto.Offset, out var parsedOffset))
            {
                return new ResponseService<AppointmentStatisticalDto>(
                    statusCode: (int)HttpStatusCode.BadRequest,
                    message: StatisticalMessages.OFFSET_MUST_BE_A_NUMBER
                );
            }
            else
            {
                offset = parsedOffset;
            }
        }
        var rawResult = await appointmentRepo.GetDoctorAppointmentStatistic(currentUserId, currentUser.Role!.RoleName, UserRolesName.Guest.ToString(), UserRolesName.Doctor.ToString(), null, null, (int)AppointmentStatus.Completed, (int)AppointmentStatus.Awaiting, (int)AppointmentStatus.Canceled, (int)AppointmentStatus.Examining, groupType, offset);
        var json = JsonSerializer.Serialize(rawResult);
        var result = JsonSerializer.Deserialize<AppointmentStatisticalDto>(json);
        return new ResponseService<AppointmentStatisticalDto>(
            statusCode: (int)HttpStatusCode.OK,
            message: StatisticalMessages.GET_STATISTICAL_APPOINTMENT_SUCCESSFULLY,
            data: result
        );

    }


    #endregion

}