using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using clinic_management.api.Models;

namespace clinic_management.api.Controllers
{
    [Route("api/medical-record")]
    [ApiController]
    public class MedicalRecordController(IMedicalRecordService medicalRecordService) : ControllerBase
    {
        #region DOCTOR EXAMINING
        [HttpPost("examining/{appointmentId}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<ResponseService<string>>> Examining([FromBody] ExaminingDto dto, [FromRoute] string appointmentId)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await medicalRecordService.ExaminingService(dto, currentUserId, appointmentId);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }

        #endregion

        #region DOCTOR UPDATE EXAMINING
        [HttpPatch("update-examining/{appointmentId}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<ResponseService<string>>> UpdateExamining([FromBody] UpdateExaminingDto dto, [FromRoute] string appointmentId)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await medicalRecordService.UpdateExaminingService(currentUserId, dto, appointmentId);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }

        #endregion

        #region GET ALL MEDICAL RECORD DETAIL
        [HttpGet("get-all-medical-record-detail")]
        [Authorize]
        public async Task<ActionResult<ResponseService<ResponsePagedService<List<MedicalRecordDetailSummaryDto>>>>> GetAllMedicalRecordDetail(PaginationDto paginationDto)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await medicalRecordService.GetAllMedicalRecordDetailService(currentUserId, paginationDto);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion

        #region GET MEDICAL RECORD DETAIL BY APPOINTMENT ID
        [HttpGet("get-medical-record-detail/{appointmentId}")]
        [Authorize]
        public async Task<ActionResult<ResponseService<GetMedicalRecordDetailDto>>> GetMedicalRecordDetailByAppointmentId(string appointmentId)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await medicalRecordService.GetMedicalRecordDetailByAppointmentIdService(currentUserId, appointmentId);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion

        #region GET ALL SERVICE
        [HttpGet("get-all-services")]
        public async Task<ActionResult<ResponseService<List<GetServiceDto>>>> GetAllServicesTest()
        {
            var result = await medicalRecordService.GetAllServicesTestService();
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion

        #region ADD NEW SERVICES IN MEDICAL RECORD DETAIL
        [HttpPost("add-new-services")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<ActionResult<ResponseService<object>>> AddNewServicesInMedicalRecord(AddNewServicesDto dto)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await medicalRecordService.AddNewServicesInMedicalRecordService(currentUserId, dto);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }

        #endregion


        #region GET MEDICAL RECORD BY MEDICAL RECORD ID
        [HttpGet("get-medical-record/{medicalRecordId}")]
        [Authorize]
        public async Task<ActionResult<ResponseService<GetMedicalRecordDto>>> GetMedicalRecordById(string medicalRecordId)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await medicalRecordService.GetMedicalRecordByIdService(currentUserId, medicalRecordId);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion

        #region GET MEDICAL RECORDS DETAIL  BY MEDICAL RECORD ID
        [HttpGet("get-medical-records-detail/{medicalRecordId}")]
        [Authorize]
        public async Task<ActionResult<ResponseService<ResponsePagedService<List<GetMedicalRecordDetailDto>>>>> GetMedicalRecordsDetailByMedicalRecordId(string medicalRecordId, [FromQuery] PaginationDto paginationDto)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await medicalRecordService.GetMedicalRecordsDetailByMedicalRecordIdService(currentUserId, medicalRecordId, paginationDto);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion
    }
}