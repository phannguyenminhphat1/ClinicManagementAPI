using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace clinic_management.api.Controllers
{
    [Route("api/medical-test")]
    [ApiController]
    public class MedicalTestController(IMedicalTestService medicalTestService) : ControllerBase
    {
        #region GET ALL MEDICAL TESTS
        [HttpGet("get-medical-tests")]
        [Authorize(Roles = "Doctor,Technician,Admin")]
        public async Task<ActionResult<ResponseService<ResponsePagedService<List<GetMedicalTestDto>>>>> GetMedicalTest([FromQuery] PaginationDto paginationDto, [FromQuery] GetMedicalTestFilterDto dto)
        {
            var result = await medicalTestService.GetMedicalTestsService(paginationDto, dto);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion

        #region GET MEDICAL TESTS RESULT BY MEDICAL TEST ID
        [HttpGet("get-medical-tests-result/{medicalTestResultId}")]
        [Authorize(Roles = "Doctor,Technician,Admin")]
        public async Task<ActionResult<ResponseService<List<SaveMedicalTestResultDto>>>> GetMedicalTestsResultById([FromRoute] string medicalTestResultId)
        {
            var result = await medicalTestService.GetMedicalTestsResultByIdService(medicalTestResultId);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion

        #region SAVE MEDICAL TEST RESULT
        [HttpPost("save-medical-test-result")]
        [Authorize(Roles = "Doctor,Technician,Admin")]
        public async Task<ActionResult<ResponseService<string>>> SaveMedicalTestResult([FromBody] List<SaveMedicalTestResultDto> dtos)
        {
            var result = await medicalTestService.SaveMedicalTestResultService(dtos);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion

        #region DELETE MEDICAL TEST RESULT
        [HttpDelete("delete-medical-test-result/{medicalTestResultId}")]
        [Authorize(Roles = "Doctor,Technician,Admin")]
        public async Task<ActionResult<ResponseService<string>>> DeleteMedicalTestResult([FromRoute] string medicalTestResultId)
        {
            var result = await medicalTestService.DeleteMedicalTestResultService(medicalTestResultId);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion

        #region CHANGE STATUS TO COMPLETE MEDICAL TEST
        [HttpPut("complete-medical-test/{medicalTestId}")]
        [Authorize(Roles = "Doctor,Technician,Admin")]
        public async Task<ActionResult<ResponseService<string>>> CompleteMedicalTest([FromRoute] string medicalTestId)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await medicalTestService.CompleteMedicalTestService(currentUserId, medicalTestId);
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