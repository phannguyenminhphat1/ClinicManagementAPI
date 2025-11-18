using System.Net;
using AutoMapper;
using clinic_management.infrastructure.Models;

public interface IMedicineService
{
    public Task<ResponseService<List<GetMedicineDto>>> GetAllMedicinesService();

}

public class MedicineService(IMedicineRepository medicineRepo, IMapper _mapper) : IMedicineService
{
    public async Task<ResponseService<List<GetMedicineDto>>> GetAllMedicinesService()
    {
        var medicines = await medicineRepo.GetAllMedicines();
        var medicinesMapper = _mapper.Map<List<GetMedicineDto>>(medicines);
        return new ResponseService<List<GetMedicineDto>>(
            statusCode: (int)HttpStatusCode.OK,
            message: PrescriptionMessages.GET_ALL_MEDICINE_SUCCESSFULLY,
            data: medicinesMapper
        );
    }
}