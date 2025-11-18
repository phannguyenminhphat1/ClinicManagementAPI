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
    public class PrescriptionController(IPrescriptionService prescriptionService) : ControllerBase
    {
        #region SAVE PRESCRIPTION
        [HttpPost("save-prescription/{medicalRecordDetailId}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<ActionResult<ResponseService<object>>> SavePrescriptionDetail([FromRoute] string medicalRecordDetailId, [FromBody] List<AddPrescriptionDetailDto> dtos)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await prescriptionService.SavePrescriptionDetailService(currentUserId, medicalRecordDetailId, dtos);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion


        #region GET PRESCRIPTION DETAILS BY PRES ID

        [HttpGet("get-prescription-details/{prescriptionId}")]
        [Authorize]
        public async Task<ActionResult<ResponseService<List<AddPrescriptionDetailDto>>>> GetPrescriptionDetailsByPresId([FromRoute] string prescriptionId)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await prescriptionService.GetPrescriptionDetailsByPresIdService(currentUserId, prescriptionId);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion

        #region GET PRESCRIPTION BY MRD ID
        [HttpGet("get-prescription/{medicalRecordDetailId}")]
        [Authorize]
        public async Task<ActionResult<ResponseService<AddPrescriptionDto>>> GetPrescriptionByMrdId([FromRoute] string medicalRecordDetailId)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await prescriptionService.GetPrescriptionByMrdIdService(currentUserId, medicalRecordDetailId);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion

        #region DELETE PRESCRIPTION DETAIL BY ID
        [HttpDelete("delete-prescription-detail/{prescriptionDetailId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<ResponseService<string>>> DeletePresDetailById([FromRoute] string prescriptionDetailId)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await prescriptionService.DeletePresDetailByIdService(currentUserId, prescriptionDetailId);
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