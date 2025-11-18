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
    public class BillingController(IBillingService billingService) : ControllerBase
    {
        #region GET ALL BILLINGS
        [HttpGet("get-all-billings")]
        [Authorize(Roles = "Receptionist,Admin")]
        public async Task<ActionResult<ResponseService<ResponsePagedService<List<GetBillingDto>>>>> GetAllBillings([FromQuery] PaginationDto paginationDto, [FromQuery] GetBillingFilterDto dto)
        {
            var result = await billingService.GetAllBillingsService(paginationDto, dto);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }

        #endregion

        #region GET BILLING BY ID
        [HttpGet("get-billing/{billingId}")]
        [Authorize(Roles = "Receptionist,Admin")]
        public async Task<ActionResult<ResponseService<GetBillingByIdDto>>> GetBilling([FromRoute] string billingId)
        {
            var result = await billingService.GetBillingService(billingId);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }
        #endregion

        #region PAYMENT
        [HttpPost("make-payment/{billingId}")]
        [Authorize(Roles = "Receptionist,Admin")]
        public async Task<ActionResult<ResponseService<object>>> MakePayment([FromRoute] string billingId, [FromBody] MakePaymentDto dto)
        {
            var result = await billingService.MakePaymentService(billingId, dto);
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