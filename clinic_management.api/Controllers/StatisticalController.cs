using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using clinic_management.api.Models;

namespace clinic_management.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticalController(IStatisticalService statisticalService) : ControllerBase
    {

        [HttpGet("get-appointment-statistic")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<ActionResult<ResponseService<AppointmentStatisticalDto>>> GetAppointmentStatistic([FromQuery] GetAppointmentStatisticDto dto)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await statisticalService.GetAppointmentStatisticService(currentUserId, dto);
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