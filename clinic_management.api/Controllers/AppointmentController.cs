using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace clinic_management.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController(IAppointmentService appointmentService) : ControllerBase
    {
        #region MAKE APPOINTMENT
        [HttpPost("make-appointment")]
        public async Task<ActionResult<ResponseService<object>>> MakeAppointment([FromBody] MakeAppointmentDto makeAppointmentDto)
        {
            var result = await appointmentService.MakeAppointmentService(makeAppointmentDto);
            return result.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion

        #region MAKE APPOINTMENT WITH DOCTOR
        [HttpPost("make-appointment-with-doctor")]
        public async Task<ActionResult<ResponseService<object>>> MakeAppointmentWithDoctor([FromBody] MakeAppointmentWithDoctorDto dto)
        {
            var result = await appointmentService.MakeAppointmentWithDoctorService(dto);
            return result.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion


        #region GET AVAIABLE TIMES ALL DOCTOR

        [HttpPost("get-available-times")]
        public async Task<ActionResult<ResponseService<List<AvailableTimeDto>>>> GetAvailableTimeSlots([FromBody] DateTimeDto dto)
        {
            var result = await appointmentService.GetAvailableTimeSlotsService(dto);
            return result.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion

        #region GET AVAIABLE TIMES SPECIFIC DOCTOR
        [HttpPost("get-available-times-doctor")]
        public async Task<ActionResult<ResponseService<List<AvailableTimeDto>>>> GetAvailableTimeSlotsDoctor([FromBody] GetAvailableTimeSlotsDoctorDto dto)
        {
            var result = await appointmentService.GetAvailableTimeSlotsDoctorService(dto.Date!, dto.DoctorId!);
            return result.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion


        #region GET APPOINTMENTS BY DATE

        #endregion
        [HttpGet("get-appointments-by-date")]
        [Authorize(Roles = "Admin,Receptionist,Doctor,Guest")]
        public async Task<ActionResult<ResponseService<List<GetAppointmentDto>>>> GetAppointmentsByDate([FromQuery] AppointmentFilterDto appointmentFilterDto)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await appointmentService.GetAppointmentsByDateService(currentUserId, appointmentFilterDto);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };

        }

        #region GET ALL APPOINTMENTS
        [HttpGet("get-appointments")]
        [Authorize(Roles = "Admin,Receptionist,Doctor,Guest")]
        public async Task<ActionResult<ResponseService<ResponsePagedService<List<GetAppointmentDto>>>>> GetAppointments([FromQuery] PaginationDto paginationDto, [FromQuery] GetAppointmentsFilterDto dto)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await appointmentService.GetAppointmentsService(currentUserId, paginationDto, dto);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };

        }
        #endregion

        #region GET APPOINTMENT BY ID
        [HttpGet("get-appointment/{appointmentId}")]
        [Authorize(Roles = "Admin,Receptionist,Doctor,Guest")]
        public async Task<ActionResult<ResponseService<GetAppointmentDto>>> GetAppointmentById([FromRoute] string appointmentId)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await appointmentService.GetAppointmentByIdService(currentUserId, appointmentId);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };

        }
        #endregion

        #region UPDATE APPOINTMENT BY ID FOR RECEPTIONIST AND ADMIN

        [HttpPatch("update-appointment/{appointmentId}")]
        [Authorize(Roles = "Receptionist,Admin")]
        public async Task<ActionResult<ResponseService<object>>> UpdateAppointment([FromBody] UpdateAppointmentDto dto, [FromRoute] string appointmentId)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await appointmentService.UpdateAppointmentService(currentUserId, dto, appointmentId);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion

        #region UPDATE APPOINTMENT STATUS BY ID FOR RECEPTIONIST AND ADMIN

        [HttpPatch("update-appointment-status/{appointmentId}")]
        [Authorize(Roles = "Receptionist,Admin")]
        public async Task<ActionResult<ResponseService<object>>> UpdateAppointmentStatus([FromBody] UpdateAppointmentStatusDto dto, [FromRoute] string appointmentId)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await appointmentService.UpdateAppointmentStatusService(currentUserId, dto, appointmentId);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion

        #region UPDATE APPOINTMENT BY ID FOR DOCTOR AND GUEST AND
        [HttpPatch("update-appointment-dag/{appointmentId}")]
        [Authorize(Roles = "Guest,Doctor")]
        public async Task<ActionResult<ResponseService<object>>> UpdateAppointmentDag([FromBody] UpdateAppointmentStatusDto dto, [FromRoute] string appointmentId)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await appointmentService.UpdateAppointmentDagService(currentUserId, dto, appointmentId);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion

        #region CHECK IN
        [HttpPost("check-in/{appointmentId}")]
        [Authorize(Roles = "Receptionist")]
        public async Task<ActionResult<ResponseService<string>>> CheckIn([FromRoute] string appointmentId)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await appointmentService.CheckInService(currentUserId, appointmentId);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion



        [HttpGet("get-appointment-histories/{appointmentId}")]
        [Authorize]
        public async Task<ActionResult<ResponseService<List<AppointmentStatusHistoryDto>>>> GetAppointmentHistories([FromRoute] string appointmentId)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await appointmentService.GetAppointmentHistoriesService(currentUserId, appointmentId);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }


    }
}