using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace clinic_management.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicineController(IMedicineService medicineService) : ControllerBase
    {

        [HttpGet("get-all-medicines")]
        [Authorize(Roles = "Doctor,Receptionist,Admin")]
        public async Task<ActionResult<ResponseService<List<GetMedicineDto>>>> GetAllMedicines()
        {
            var result = await medicineService.GetAllMedicinesService();
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