using System.Net;
using System.Text.Json;
using AutoMapper;
using clinic_management.infrastructure.Models;
using CloudinaryDotNet.Actions;

public interface IPrescriptionService
{
    public Task<ResponseService<string>> SavePrescriptionDetailService(Guid currentUserId, string medicalRecordDetailId, List<AddPrescriptionDetailDto> dtos);
    public Task<ResponseService<List<AddPrescriptionDetailDto>>> GetPrescriptionDetailsByPresIdService(Guid currentUserId, string prescriptionId);
    public Task<ResponseService<AddPrescriptionDto>> GetPrescriptionByMrdIdService(Guid currentUserId, string medicalRecordDetailId);
    public Task<ResponseService<string>> DeletePresDetailByIdService(Guid currentUserId, string prescriptionDetailId);


}

public class PrescriptionService(IMapper _mapper, IPrescriptionRepository prescriptionRepo, IPrescriptionDetailRepository prescriptionDetailRepo, IUnitOfWork unitOfWork, IUserRepository userRepo, IMedicineRepository medicineRepo, IBillingRepository billingRepo, IBillingMedicineRepository billingMedicineRepo, IMedicalRecordDetailRepository medicalRecordDetailRepo, IAppointmentStatusHistoryRepository appointmentStatusHistoryRepo) : IPrescriptionService
{
    #region GET OR CREATE PRESCRIPTION
    private async Task<ResponseService<Prescription>> GetOrCreatePrescription(Guid currentUserId, int medicalRecordDetailId)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
        {
            return new ResponseService<Prescription>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }
        var pres = await prescriptionRepo.GetPrescriptionByMrdId(currentUserId, currentUser.Role!.RoleName, UserRolesName.Guest.ToString(), UserRolesName.Doctor.ToString(), medicalRecordDetailId);
        if (pres != null)
        {
            return new ResponseService<Prescription>(
                statusCode: (int)HttpStatusCode.OK,
                message: PrescriptionMessages.GET_PRESCRIPTION_SUCCESSFULLY,
                data: pres
            );
        }

        pres = new Prescription
        {
            MedicalRecordDetailId = medicalRecordDetailId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await prescriptionRepo.AddAsync(pres);
        return new ResponseService<Prescription>(
            statusCode: (int)HttpStatusCode.Created,
            message: PrescriptionMessages.GET_PRESCRIPTION_SUCCESSFULLY,
            data: pres
        );
    }
    #endregion


    #region SAVE PRESCRIPTION DETAIL

    public async Task<ResponseService<string>> SavePrescriptionDetailService(Guid currentUserId, string medicalRecordDetailId, List<AddPrescriptionDetailDto> dtos)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }
        if (dtos == null || dtos.Count == 0)
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: PrescriptionMessages.LIST_PRESCRIPTION_DETAILS_IS_REQUIRED
            );
        }
        if (!int.TryParse(medicalRecordDetailId, out int parsedMedicalRecordDetailId))
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: MedicalRecordMessages.MEDICAL_RECORD_DETAIL_MUST_BE_A_NUMBER
            );
        }
        var prescription = await GetOrCreatePrescription(currentUserId, parsedMedicalRecordDetailId);
        var medicalRecordDetail = await medicalRecordDetailRepo.GetMedicalRecordDetail(x => x.MedicalRecordDetailId == parsedMedicalRecordDetailId, currentUserId, currentUser.Role!.RoleName, UserRolesName.Guest.ToString(), UserRolesName.Doctor.ToString());
        if (medicalRecordDetail == null)
        {

            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: MedicalRecordMessages.MEDICAL_RECORD_DETAIL_NOT_FOUND
            );
        }

        var billing = await billingRepo.GetBillingByAppointmentId(medicalRecordDetail.AppointmentId!.Value);
        if (billing == null)
        {

            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: BillingMessages.BILLING_NOT_FOUND
            );
        }

        foreach (var item in dtos)
        {
            // Validate cơ bản
            if (!int.TryParse(item.MedicineId, out int medicineId))
            {
                return new ResponseService<string>(
                    (int)HttpStatusCode.BadRequest,
                    PrescriptionMessages.MEDICINE_ID_MUST_BE_A_NUMBER
                );
            }

            if (!int.TryParse(item.Quantity, out int quantity))
            {
                return new ResponseService<string>(
                    (int)HttpStatusCode.BadRequest,
                    PrescriptionMessages.QUANTITY_MUST_BE_A_NUMBER
                );
            }
            if (quantity <= 0)
            {
                return new ResponseService<string>(
                    (int)HttpStatusCode.BadRequest,
                    PrescriptionMessages.QUANTITY_MUST_BE_POSITIVE
                );
            }
            var medicine = await medicineRepo.GetMedicineById(medicineId);
            if (medicine == null)
            {
                return new ResponseService<string>(
                    statusCode: (int)HttpStatusCode.NotFound,
                    message: PrescriptionMessages.MEDICINE_NOT_FOUND
                );
            }
            if (medicine.Stock < quantity)
            {
                return new ResponseService<string>(
                    statusCode: (int)HttpStatusCode.BadRequest,
                    message: $"{PrescriptionMessages.QUANTITY_IN_STOCK_IS_LESS_THAN_YOUR_INPUT_QUANTITY}: chỉ còn: {medicine.Stock} {medicine.Unit}"
                );
            }
            if (item.PrescriptionDetailId.HasValue)
            {
                var presDetail = await prescriptionDetailRepo.GetPrescriptionDetailById(item.PrescriptionDetailId.Value);
                if (presDetail == null)
                {
                    return new ResponseService<string>(
                        statusCode: (int)HttpStatusCode.NotFound,
                        message: PrescriptionMessages.PRESCRIPTION_NOT_FOUND
                    );
                }
                presDetail.Quantity = quantity;
                presDetail.Usage = item.Usage!;
                presDetail.MedicineId = medicine.MedicineId;
                presDetail.Prescription = prescription.Data!;
                await prescriptionDetailRepo.Update(presDetail);
                continue;
            }

            var newPresDetail = new PrescriptionDetail
            {
                MedicineId = medicine.MedicineId,
                Quantity = quantity,
                Usage = item.Usage!,
                Prescription = prescription.Data!
            };

            await billingMedicineRepo.AddAsync(new BillingMedicine
            {
                Billing = billing,
                MedicineId = medicine.MedicineId,
                PaymentStatusId = (int)PaymentStatusEnum.Unpaid,
                Price = medicine.Price,
                Quantity = quantity
            });
            await prescriptionDetailRepo.AddAsync(newPresDetail);
            billing.TotalAmount += medicine.Price * quantity;
        }
        billing.UpdatedAt = DateTime.UtcNow;
        await billingRepo.Update(billing);

        await appointmentStatusHistoryRepo.AddAsync(new AppointmentStatusHistory
        {
            StatusId = (int)AppointmentStatus.Completed,
            AppointmentId = medicalRecordDetail.AppointmentId!.Value,
            Note = AppointmentMessages.ADD_PRESCRIPTION
        });

        await unitOfWork.SaveChangesAsync();
        return new ResponseService<string>(
            statusCode: (int)HttpStatusCode.OK,
            message: PrescriptionMessages.SAVE_PRESCRIPTION_SUCCESSFULLY
        );

    }
    #endregion

    #region GET PRESCRIPTION DETAILS BY PRES ID
    public async Task<ResponseService<List<AddPrescriptionDetailDto>>> GetPrescriptionDetailsByPresIdService(Guid currentUserId, string prescriptionId)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
        {
            return new ResponseService<List<AddPrescriptionDetailDto>>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }
        if (!int.TryParse(prescriptionId, out var parsedPrescriptionId))
        {
            return new ResponseService<List<AddPrescriptionDetailDto>>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: PrescriptionMessages.PRESCRIPTION_ID_MUST_BE_A_NUMBER
            );
        }
        var pres = await prescriptionRepo.GetPrescriptionById(parsedPrescriptionId);
        if (pres == null)
        {
            return new ResponseService<List<AddPrescriptionDetailDto>>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: PrescriptionMessages.PRESCRIPTION_NOT_FOUND
            );
        }
        var lstPresDetail = await prescriptionDetailRepo.GetListPrescriptionDetailByPresId(currentUserId, currentUser.Role!.RoleName, UserRolesName.Guest.ToString(), UserRolesName.Doctor.ToString(), parsedPrescriptionId);
        var lstPresDetailMapper = _mapper.Map<List<AddPrescriptionDetailDto>>(lstPresDetail);
        return new ResponseService<List<AddPrescriptionDetailDto>>(
            statusCode: (int)HttpStatusCode.OK,
            message: PrescriptionMessages.GET_LIST_PRESCRIPTION_DETAIL_SUCCESSFULLY,
            data: lstPresDetailMapper
        );
    }
    #endregion

    #region GET PRES BY MEDICAL RECORD DETAIL ID
    public async Task<ResponseService<AddPrescriptionDto>> GetPrescriptionByMrdIdService(Guid currentUserId, string medicalRecordDetailId)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
        {
            return new ResponseService<AddPrescriptionDto>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }
        if (!int.TryParse(medicalRecordDetailId, out var parsedMedicalRecordDetailId))
        {
            return new ResponseService<AddPrescriptionDto>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: MedicalRecordMessages.MEDICAL_RECORD_DETAIL_MUST_BE_A_NUMBER
            );
        }
        var pres = await prescriptionRepo.GetPrescriptionByMrdId(currentUserId, currentUser.Role!.RoleName, UserRolesName.Guest.ToString(), UserRolesName.Doctor.ToString(), parsedMedicalRecordDetailId);
        var presMapper = pres != null ? _mapper.Map<AddPrescriptionDto>(pres) : null;
        return new ResponseService<AddPrescriptionDto>(
            statusCode: (int)HttpStatusCode.OK,
            message: PrescriptionMessages.GET_PRESCRIPTION_SUCCESSFULLY,
            data: presMapper
        );

    }


    #endregion

    #region DELETE PRESCRIPTION DETAIL BY ID
    public async Task<ResponseService<string>> DeletePresDetailByIdService(Guid currentUserId, string prescriptionDetailId)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }
        if (!int.TryParse(prescriptionDetailId, out var parsedPrescriptionDetailId))
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: PrescriptionMessages.PRESCRIPTION_DETAIL_ID_MUST_BE_A_NUMBER
            );
        }
        var prescriptionDetail = await prescriptionDetailRepo.GetPrescriptionDetailById(parsedPrescriptionDetailId);
        if (prescriptionDetail == null)
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: PrescriptionMessages.PRESCRIPTION_NOT_FOUND
            );
        }
        await prescriptionDetailRepo.DeleteAsync(prescriptionDetail.PrescriptionDetailId);
        await unitOfWork.SaveChangesAsync();
        return new ResponseService<string>(
           statusCode: (int)HttpStatusCode.OK,
           message: PrescriptionMessages.DELETE_PRES_DETAIL_SUCCESSFULLY
       );
    }

    #endregion

}