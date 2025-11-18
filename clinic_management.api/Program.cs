using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using clinic_management.infrastructure.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// service EF
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ClinicManagementContext>(options =>
    options.UseSqlServer(connectionString));
// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["AppSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["AppSettings:Audience"],
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Key"]!)),
        ValidateIssuerSigningKey = true,
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name
    };
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse(); // Chặn phản hồi mặc định
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new
            {
                message = AuthMessages.ACCESS_TOKEN_IS_REQUIRED
            });
            await context.Response.WriteAsync(result);
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new
            {
                message = AuthMessages.FORBIDDEN
            });
            await context.Response.WriteAsync(result);
        }
    };
});
builder.Services.AddAuthorization();
builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // ❗ Đừng dùng CamelCase
    });

// Auto Mapper
builder.Services.AddAutoMapper(typeof(MapperTool).Assembly);

// Repository
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentStatusHistoryRepository, AppointmentStatusHistoryRepository>();
builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
builder.Services.AddScoped<IBillingRepository, BillingRepository>();
builder.Services.AddScoped<IBillingDetailRepository, BillingDetailRepository>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentStatusRepository, PaymentStatusRepository>();
builder.Services.AddScoped<IPaymentDetailRepository, PaymentDetailRepository>();
builder.Services.AddScoped<IMedicalRecordDetailRepository, MedicalRecordDetailRepository>();
builder.Services.AddScoped<IMedicalTestResultRepository, MedicalTestResultRepository>();
builder.Services.AddScoped<IMedicalTestRepository, MedicalTestRepository>();
builder.Services.AddScoped<IMedicalRecordSummaryRepository, MedicalRecordSummaryRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IPrescriptionDetailRepository, PrescriptionDetailRepository>();
builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();
builder.Services.AddScoped<IBillingMedicineRepository, BillingMedicineRepository>();



// Service
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStatisticalService, StatisticalService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IChattingService, ChattingService>();
builder.Services.AddScoped<IMedicalTestService, MedicalTestService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IMedicineService, MedicineService>();



// Config default response
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var modelType = context.ActionDescriptor.Parameters
            .FirstOrDefault(p => p.BindingInfo?.BindingSource == BindingSource.Body)
            ?.ParameterType;

        var jsonNameMap = modelType?
            .GetProperties()
            .ToDictionary(
                prop => prop.Name,
                prop => prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ??
                        char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1)
            ) ?? new Dictionary<string, string>();

        var errors = context.ModelState
            .Where(e => e.Value!.Errors.Count > 0)
            .ToDictionary(
                kvp => jsonNameMap.ContainsKey(kvp.Key) ? jsonNameMap[kvp.Key] : kvp.Key,
                kvp => kvp.Value!.Errors.First().ErrorMessage
            );

        var response = new ResponseService<object>(
            statusCode: StatusCodes.Status422UnprocessableEntity,
            message: CommonMessages.ERROR,
            errors: errors
        );

        return new UnprocessableEntityObjectResult(response);
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>(); // Lấy thông tin lỗi từ pipeline
        context.Response.ContentType = "application/json"; // Định dạng Response dưới dạng JSON
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        var errorResponse = new
        {
            // message = exceptionFeature?.Error.Message ?? CommonMessages.ERROR
            message = CommonMessages.ERROR

        };
        await context.Response.WriteAsJsonAsync(errorResponse);
    });
});
app.UseHttpsRedirection();
app.MapControllers();
app.Run();