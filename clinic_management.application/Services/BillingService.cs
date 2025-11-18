using System.Net;
using AutoMapper;
using clinic_management.infrastructure.Models;

public interface IBillingService
{
    public Task<ResponseService<ResponsePagedService<List<GetBillingDto>>>> GetAllBillingsService(PaginationDto paginationDto, GetBillingFilterDto dto);

    public Task<ResponseService<GetBillingByIdDto>> GetBillingService(string billingId);

    public Task<ResponseService<object>> MakePaymentService(string billingId, MakePaymentDto dto);



}

public class BillingService(IBillingRepository billingRepo, IUnitOfWork unitOfWork, IPaymentRepository paymentRepo, IPaymentDetailRepository paymentDetailRepo, IBillingDetailRepository billingDetailRepo, IBillingMedicineRepository billingMedicineRepo, IMapper _mapper) : IBillingService
{
    #region GET ALL BILLINGS
    public async Task<ResponseService<ResponsePagedService<List<GetBillingDto>>>> GetAllBillingsService(PaginationDto paginationDto, GetBillingFilterDto dto)
    {
        if (!ValidateAndParsePagination.TryParsePagination<ResponsePagedService<List<GetBillingDto>>>(paginationDto.Page, paginationDto.PageSize, out int? pageParsed, out int? pageSizeParsed, out var errorResponsePaged))
        {
            return errorResponsePaged!;
        }
        int currentPage = pageParsed ?? 1;
        int pageSize = pageSizeParsed ?? 10;
        int? parsedStatusId = null;
        DateTime? date = null;

        // Nếu có truyền date
        if (!string.IsNullOrWhiteSpace(dto.Date))
        {
            if (!DateTime.TryParseExact(dto.Date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
            {
                return new ResponseService<ResponsePagedService<List<GetBillingDto>>>(
                    statusCode: (int)HttpStatusCode.BadRequest,
                    message: AppointmentMessages.FILTER_DATE_IS_INVALID
                );
            }
            date = parsedDate;
        }

        // Validate StatusId
        if (!string.IsNullOrWhiteSpace(dto.StatusId))
        {
            if (!ValidateAndParseStatus.TryParseStatus<ResponsePagedService<List<GetBillingDto>>>(dto.StatusId, out parsedStatusId, out var errorResponseStatus))
            {
                return errorResponseStatus!;
            }
            if (parsedStatusId.HasValue)
            {
                if (!EnumValidationService.IsValid<PaymentStatusEnum>(parsedStatusId.Value))
                {
                    return new ResponseService<ResponsePagedService<List<GetBillingDto>>>(
                        statusCode: (int)HttpStatusCode.OK,
                        message: $"{AppointmentMessages.STATUS_ID_IS_INVALID}: {EnumValidationService.GetValidEnumValues<PaymentStatusEnum>()}"
                    );
                }
            }
        }

        var (billings, totalRecords) = await billingRepo.GetAllBillings(currentPage, pageSize, date, parsedStatusId, dto.Keyword);
        int totalPage = (int)Math.Ceiling((double)totalRecords / pageSize);
        var billingsMapper = _mapper.Map<List<GetBillingDto>>(billings);
        var result = new ResponsePagedService<List<GetBillingDto>>
        (
            data: billingsMapper,
            currentPage: currentPage,
            pageSize: pageSize,
            totalPage: totalPage,
            totalItem: totalRecords
        );
        return new ResponseService<ResponsePagedService<List<GetBillingDto>>>(
            statusCode: (int)HttpStatusCode.OK,
            message: BillingMessages.GET_BILLINGS_SUCCESSFULLY,
            data: result
        );

    }

    #endregion

    #region GET BILLING BY ID
    public async Task<ResponseService<GetBillingByIdDto>> GetBillingService(string billingId)
    {
        if (!int.TryParse(billingId, out var parsedBillingId))
        {
            return new ResponseService<GetBillingByIdDto>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: BillingMessages.BILLING_ID_MUST_BE_A_NUMBER
            );
        }
        var billing = await billingRepo.GetBilling(parsedBillingId);
        if (billing is null)
        {
            return new ResponseService<GetBillingByIdDto>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: BillingMessages.BILLING_NOT_FOUND
            );
        }
        var billingMapper = _mapper.Map<GetBillingByIdDto>(billing);
        return new ResponseService<GetBillingByIdDto>(
            statusCode: (int)HttpStatusCode.OK,
            message: BillingMessages.GET_BILLING_SUCCESSFULLY,
            data: billingMapper
        );
    }
    #endregion


    #region MAKE PAYMENT
    public async Task<ResponseService<object>> MakePaymentService(string billingId, MakePaymentDto dto)
    {
        if (!int.TryParse(billingId, out var parsedBillingId))
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: BillingMessages.BILLING_ID_MUST_BE_A_NUMBER
            );
        }
        var billing = await billingRepo.GetBilling(parsedBillingId);
        if (billing is null)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: BillingMessages.BILLING_NOT_FOUND
            );
        }
        var unpaidDetails = await billingDetailRepo.GetUnpaidBillingDetailsByBillingId(parsedBillingId, (int)PaymentStatusEnum.Unpaid);
        var unpaidBillingMedicines = await billingMedicineRepo.GetUnpaidBillingMedicineByBillingId(parsedBillingId, (int)PaymentStatusEnum.Unpaid);

        if (!unpaidDetails.Any() && !unpaidBillingMedicines.Any())
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: BillingMessages.ALL_BILLING_DETAILS_IN_BILLING_IS_ALREADY_PAID
            );
        }
        var totalUnpaidAmount = unpaidDetails.Sum(d => d.Price);
        var totalUnpaidMedicinesAmount = unpaidBillingMedicines.Sum(d => d.Quantity * d.Price);
        var totalAmount = totalUnpaidAmount + totalUnpaidMedicinesAmount;


        // Validate Payment Status Id
        var errors = new Dictionary<string, string>();
        if (!ValidateAndParseStatus.TryParseStatus<object>(dto.PaymentMethodId, out int? parsedPaymentMethodId, out var errorResponseStatus, isFromBody: true))
        {
            return errorResponseStatus!;
        }
        if (parsedPaymentMethodId.HasValue)
        {
            if (!EnumValidationService.IsValid<PaymentMethodEnum>(parsedPaymentMethodId.Value))
            {
                errors["payment_method_id"] = $"{BillingMessages.PAYMENT_METHOD_ID_IS_INVALID}: {EnumValidationService.GetValidEnumValues<PaymentMethodEnum>()}";
            }
        }

        if (errors.Count > 0)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.UnprocessableEntity,
                message: AppointmentMessages.ERROR,
                errors: errors
            );
        }

        // 1. Tạo payment
        var newPayment = new Payment
        {
            Billing = billing,
            Amount = totalAmount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await paymentRepo.AddAsync(newPayment);


        // 2. Tạo payment_details và update billing_details
        foreach (var detail in unpaidDetails)
        {
            var newDetail = new PaymentDetail
            {
                Payment = newPayment,
                BillingDetailId = detail.BillingDetailId,
                PaymentMethodId = parsedPaymentMethodId!.Value,
                Amount = detail.Price,
                PaymentDate = DateTime.UtcNow
            };

            await paymentDetailRepo.AddAsync(newDetail);

            // Cập nhật trạng thái billing_detail
            detail.PaymentStatusId = (int)PaymentStatusEnum.Paid;
        }

        foreach (var medicine in unpaidBillingMedicines)
        {
            var medicinePaymentDetail = new PaymentDetail
            {
                Payment = newPayment,
                BillingMedicineId = medicine.BillingMedicineId,
                PaymentMethodId = parsedPaymentMethodId!.Value,
                Amount = medicine.Price * medicine.Quantity,
                PaymentDate = DateTime.UtcNow
            };

            await paymentDetailRepo.AddAsync(medicinePaymentDetail);
            medicine.PaymentStatusId = (int)PaymentStatusEnum.Paid;
        }
        await unitOfWork.SaveChangesAsync();

        return new ResponseService<object>(
            statusCode: (int)HttpStatusCode.OK,
            message: $"Đã thanh toán {totalAmount:N0}đ cho hóa đơn #{billing.BillingId}"
        );
    }


    #endregion

}