using System.Text.Json;
using AutoMapper;
using clinic_management.infrastructure.Models;

public class JsonToListConverter<T> : IValueConverter<string?, List<T>?>
{
    public List<T>? Convert(string? sourceMember, ResolutionContext context)
    {
        return string.IsNullOrEmpty(sourceMember)
            ? new List<T>()
            : JsonSerializer.Deserialize<List<T>>(sourceMember);
    }
}
public class MapperTool : Profile
{
    public MapperTool()
    {
        CreateMap<UserLoginDto, User>().ReverseMap();
        CreateMap<UserRegisterDto, User>().ReverseMap();
        CreateMap<PatientDto, User>().ReverseMap();
        CreateMap<StatusDto, Status>().ReverseMap();
        CreateMap<Specialty, SpecialtyDto>().ReverseMap();
        CreateMap<User, AddUserDto>().ReverseMap();
        CreateMap<User, ReceptionistDto>().ReverseMap();
        CreateMap<User, UserChattingDto>().ReverseMap();
        CreateMap<Service, GetServiceDto>().ReverseMap();
        CreateMap<User, DoctorDto>()
            .ForMember(dest => dest.SpecialtyName, opt => opt.MapFrom(src => src.Specialty!.SpecialtyName))
            .ReverseMap();
        CreateMap<Appointment, GetAppointmentDto>()
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status!.StatusName))
            .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.Status!.StatusId))

            .ReverseMap();
        CreateMap<User, GetMeDto>()
            .ForMember(dest => dest.SpecialtyName, opt => opt.MapFrom(src => src.Specialty!.SpecialtyName))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role!.RoleName))
            .ReverseMap();
        CreateMap<User, GetUsersAdminDto>()
            .ForMember(dest => dest.SpecialtyName, opt => opt.MapFrom(src => src.Specialty!.SpecialtyName))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role!.RoleName))
            .ReverseMap();
        CreateMap<User, GetUsersDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role!.RoleName))
            .ReverseMap();

        CreateMap<User, GetUsersMedicalRecordDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role!.RoleName))
            .ForMember(dest => dest.MedicalRecordId, opt => opt.MapFrom(src => src.MedicalRecord!.MedicalRecordId))
            .ForMember(dest => dest.MedicalRecordDetailId, opt => opt.MapFrom(src => src.MedicalRecord!.MedicalRecordDetails.Select(mrd => mrd.MedicalRecordDetailId)))

            .ReverseMap();
        CreateMap<MedicalRecordSummary, MedicalRecordDetailSummaryDto>()
           .ForMember(dest => dest.Tests,
               opt => opt.ConvertUsing(new JsonToListConverter<TestDto>(), src => src.Tests))
           .ForMember(dest => dest.Prescriptions,
               opt => opt.ConvertUsing(new JsonToListConverter<PrescriptionDto>(), src => src.Prescriptions))
            .ReverseMap();
        CreateMap<Billing, GetBillingDto>()
            .ReverseMap();
        CreateMap<Billing, GetBillingByIdDto>()
            .ReverseMap();
        CreateMap<BillingDetail, GetBillingDetailDto>()
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.PaymentStatus!.StatusName))
            .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service!.ServiceName))
            .ReverseMap();
        CreateMap<BillingMedicine, GetBillingMedicineDto>()
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.PaymentStatus!.StatusName))
            .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine!.MedicineName))
            .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Medicine!.Unit))
            .ReverseMap();
        CreateMap<Conversation, GetConversationDto>().ReverseMap();
        CreateMap<Message, GetMessageDto>().ReverseMap();
        CreateMap<GetMedicalTestResultDto, MedicalTestResult>().ReverseMap();
        CreateMap<SaveMedicalTestResultDto, MedicalTestResult>().ReverseMap();
        CreateMap<GetMedicalTestDto, MedicalTest>().ReverseMap()
            .ForMember(dest => dest.Fullname, opt => opt.MapFrom(src => src.MedicalRecordDetail!.Appointment!.Patient!.Fullname))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status!.StatusName));
        CreateMap<GetMedicalRecordDetailDto, MedicalRecordDetail>().ReverseMap();
        CreateMap<GetMedicalRecordDto, MedicalRecord>().ReverseMap();
        CreateMap<GetMedicalRecordDetailHistoryDto, MedicalRecordDetail>().ReverseMap();
        CreateMap<GetMedicineDto, Medicine>().ReverseMap();
        CreateMap<PrescriptionDetail, AddPrescriptionDetailDto>()
            .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Medicine!.Unit))
            .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine!.MedicineName))
            .ReverseMap();
        CreateMap<AddPrescriptionDto, Prescription>()
            .ReverseMap();
        CreateMap<AppointmentStatusHistory, AppointmentStatusHistoryDto>()
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.StatusName))
            .ReverseMap();






    }
}