using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AutoMapper;
using clinic_management.infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
public interface IUserService
{
    public Task<ResponseService<TokenResponse>> LoginService(UserLoginDto userLoginDto);
    public Task<ResponseService<TokenResponse>> RefreshTokenService(Guid userId, RefreshTokenDto refreshTokenDto);
    public Task<ResponseService<string>> LogoutService(Guid userId, RefreshTokenDto refreshTokenDto);
    public Task<ResponseService<ResponsePagedService<List<GetUsersAdminDto>>>> GetUsersForAdminService(UserQueryFilterDto filterDto, PaginationDto paginationDto);
    public Task<ResponseService<ResponsePagedService<List<GetUsersMedicalRecordDto>>>> GetUsersForReceptionistService(UserQueryFilterReDto filterDto, PaginationDto paginationDto);
    public Task<ResponseService<ResponsePagedService<List<GetUsersMedicalRecordDto>>>> GetUsersForDoctorService(UserQueryFilterReDto filterDto, PaginationDto paginationDto, Guid currentUserId);
    public Task<ResponseService<List<DoctorDto>>> GetDoctorsPublicService();
    public Task<ResponseService<List<GetUsersDto>>> GetAllDoctors();
    public Task<ResponseService<List<GetUsersDto>>> GetReceptionistsService();
    public Task<ResponseService<GetMeDto>> GetUserByIdService(Guid currentUserId, string userId);
    public Task<ResponseService<GetMeDto>> GetMeService(Guid currentUserId);
    public ResponseService<string> UploadFileService(UploadFileDto dto);
    public Task<ResponseService<object>> UpdateMeService(Guid currentUserId, UpdateMeDto dto);
    public Task<ResponseService<object>> UpdateUserService(Guid currentUserId, UpdateUserDto dto, string userId);
    public Task<ResponseService<object>> AddUserForReceptionistService(AddUserDto dto);
    public Task<ResponseService<object>> AddUserForAdminService(AddUserDto dto);
}
public class UserService(IUserRepository userRepo, IMapper _mapper, IConfiguration _configuration, IRefreshTokenRepository refreshTokenRepo, IUnitOfWork unitOfWork, IAppointmentRepository appointmentRepo) : IUserService
{

    #region LOGIN
    public async Task<ResponseService<TokenResponse>> LoginService(UserLoginDto userLoginDto)
    {
        var user = await userRepo.GetUserWithRoleAsync(u => u.Phone == userLoginDto.Phone);
        if (user is null)
        {
            return new ResponseService<TokenResponse>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: AuthMessages.PHONE_OR_PASSWORD_IS_INCORRECT
            );

        }
        string accessToken = CreateAccessToken(user);
        string resfreshToken = await GenerateAndSaveRefreshTokenAsync(user.UserId);
        return new ResponseService<TokenResponse>(
            statusCode: (int)HttpStatusCode.OK,
            message: AuthMessages.LOGIN_SUCCESSFULLY,
            data: new TokenResponse(accessToken, resfreshToken)
        );
    }
    #endregion


    #region LOGOUT
    public async Task<ResponseService<string>> LogoutService(Guid userId, RefreshTokenDto refreshTokenDto)
    {
        var existingToken = await refreshTokenRepo.SingleOrDefaultAsync(rt => rt.Token == refreshTokenDto.Token && rt.UserId == userId);
        if (existingToken is null)
        {
            return new ResponseService<string>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: AuthMessages.REFRESH_TOKEN_NOT_FOUND
            );
        }
        await refreshTokenRepo.DeleteRefreshTokenStringAsync(existingToken.RefreshTokenId);
        await unitOfWork.SaveChangesAsync();

        return new ResponseService<string>(
           statusCode: (int)HttpStatusCode.OK,
           message: AuthMessages.LOGOUT_SUCCESSFULLY
       );
    }
    #endregion

    #region REFRESH TOKEN
    public async Task<ResponseService<TokenResponse>> RefreshTokenService(Guid userId, RefreshTokenDto refreshTokenDto)
    {
        var tokenEntity = await ValidateRefreshTokenAsync(userId, refreshTokenDto.Token);

        if (tokenEntity is null)
        {
            return new ResponseService<TokenResponse>(statusCode: (int)HttpStatusCode.BadRequest, message: AuthMessages.INVALID_REFRESH_TOKEN);
        }

        // Xóa refresh token cũ
        await refreshTokenRepo.DeleteRefreshTokenStringAsync(tokenEntity.RefreshTokenId);
        await unitOfWork.SaveChangesAsync();

        var user = await userRepo.GetUserWithRoleAsync(u => u.UserId == tokenEntity.UserId);
        string accessToken = CreateAccessToken(user!);
        string resfreshToken = await GenerateAndSaveRefreshTokenAsync(user!.UserId);
        return new ResponseService<TokenResponse>(
            statusCode: (int)HttpStatusCode.Created,
            message: AuthMessages.REFRESH_TOKEN_SUCCESSFULLY,
            data: new TokenResponse(accessToken, resfreshToken)
        );
    }
    #endregion


    #region CREATE ACCESS TOKEN
    private string CreateAccessToken(User user)
    {

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier,user.UserId.ToString()),
            new Claim(ClaimTypes.Role,user.Role!.RoleName.ToString()!),
            new Claim(ClaimTypes.Name,user.Fullname!),

        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Key")!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        var tokenDescriptor = new JwtSecurityToken(
            issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
            audience: _configuration.GetValue<string>("AppSettings:Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds

        );
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
    #endregion

    #region CREATE REFRESH TOKEN AND SAVE
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task<string> GenerateAndSaveRefreshTokenAsync(Guid userId)
    {
        var generateRefreshToken = GenerateRefreshToken();

        RefreshToken refreshToken = new RefreshToken
        {
            Token = generateRefreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresTime = DateTime.UtcNow.AddDays(7),
            UserId = userId,
        };
        await refreshTokenRepo.AddAsync(refreshToken);
        await unitOfWork.SaveChangesAsync();
        return generateRefreshToken;
    }
    #endregion

    #region VALIDATE TOKEN
    private async Task<RefreshToken?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
    {
        var tokenEntity = await refreshTokenRepo.SingleOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);
        if (tokenEntity is null || tokenEntity.ExpiresTime <= DateTime.UtcNow)
        {
            return null;
        }
        return tokenEntity;
    }
    #endregion


    #region GET USERS
    // Admin
    public async Task<ResponseService<ResponsePagedService<List<GetUsersAdminDto>>>> GetUsersForAdminService(UserQueryFilterDto filterDto, PaginationDto paginationDto)
    {
        if (!ValidateAndParsePagination.TryParsePagination<ResponsePagedService<List<GetUsersAdminDto>>>(paginationDto.Page, paginationDto.PageSize, out int? pageParsed, out int? pageSizeParsed, out var errorResponsePaged))
        {
            return errorResponsePaged!;
        }

        // Validate Role (Enum)
        if (!ValidateAndParseRole.TryParseRole<ResponsePagedService<List<GetUsersAdminDto>>>(filterDto.Role, out var parsedRole, out var errorResponse))
        {
            return errorResponse!;
        }

        // Filter theo Role nếu có
        if (parsedRole.HasValue)
        {
            if (!EnumValidationService.IsValid<UserRole>(parsedRole.Value))
            {
                return new ResponseService<ResponsePagedService<List<GetUsersAdminDto>>>(
                    statusCode: (int)HttpStatusCode.BadRequest,
                    message: $@"{UserMessages.ROLE_IS_INVALID}: {EnumValidationService.GetValidEnumValues<UserRole>()}"
                );
            }
        }

        // Filter theo Specialty nếu là bác sĩ
        if (!ValidateAndParseSpecialty.TryParseSpecialty<ResponsePagedService<List<GetUsersAdminDto>>>(filterDto.Specialty, out var parsedSpecialty, out var errorResponseSpecialty))
        {
            return errorResponseSpecialty!;
        }
        if (parsedSpecialty is not null && parsedRole is null)
        {
            return new ResponseService<ResponsePagedService<List<GetUsersAdminDto>>>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: UserMessages.ONLY_ROLE_DOCTOR_HAS_SPECIALTY
            );
        }
        if (parsedSpecialty is not null)
        {
            if (parsedRole.HasValue && parsedRole.Value != (int)UserRole.Doctor)
            {
                return new ResponseService<ResponsePagedService<List<GetUsersAdminDto>>>(
                    statusCode: (int)HttpStatusCode.BadRequest,
                    message: UserMessages.ONLY_ROLE_DOCTOR_HAS_SPECIALTY
                );
            }
            if (!EnumValidationService.IsValid<SpecialtyEnum>(parsedSpecialty.Value))
            {
                return new ResponseService<ResponsePagedService<List<GetUsersAdminDto>>>(
                    statusCode: (int)HttpStatusCode.BadRequest,
                    message: $@"{UserMessages.SPECIALTY_IS_INVALID}: {EnumValidationService.GetValidEnumValues<SpecialtyEnum>()}"
                );
            }

        }

        int currentPage = pageParsed ?? 1;
        int pageSize = pageSizeParsed ?? 10;
        var (users, totalRecords) = await userRepo.GetUsersForAdminAsync(parsedRole, filterDto.Name, filterDto.Phone, parsedSpecialty, currentPage, pageSize);
        int totalPage = (int)Math.Ceiling((double)totalRecords / pageSize);
        var usersDto = _mapper.Map<List<GetUsersAdminDto>>(users);
        var resultResponse = new ResponsePagedService<List<GetUsersAdminDto>>(
            data: usersDto,
            currentPage: currentPage,
            pageSize: pageSize,
            totalPage: totalPage,
            totalItem: totalRecords
        );
        return new ResponseService<ResponsePagedService<List<GetUsersAdminDto>>>(
            statusCode: (int)HttpStatusCode.OK,
            message: UserMessages.GET_USERS_SUCCESSFULLY,
            data: resultResponse
        );
    }

    // Receiptionist
    public async Task<ResponseService<ResponsePagedService<List<GetUsersMedicalRecordDto>>>> GetUsersForReceptionistService(UserQueryFilterReDto filterDto, PaginationDto paginationDto)
    {
        byte? parsedGender = null;
        if (!ValidateAndParsePagination.TryParsePagination<ResponsePagedService<List<GetUsersMedicalRecordDto>>>(paginationDto.Page, paginationDto.PageSize, out int? pageParsed, out int? pageSizeParsed, out var errorResponsePaged))
        {
            return errorResponsePaged!;
        }

        if (!string.IsNullOrEmpty(filterDto.Gender))
        {
            if (byte.TryParse(filterDto.Gender, out byte genderValue))
            {
                parsedGender = genderValue;
            }
            else
            {
                return new ResponseService<ResponsePagedService<List<GetUsersMedicalRecordDto>>>(
                    statusCode: (int)HttpStatusCode.BadRequest,
                    message: UserMessages.GENDER_IS_INVALID
                );
            }
        }

        int currentPage = pageParsed ?? 1;
        int pageSize = pageSizeParsed ?? 10;
        var (users, totalRecords) = await userRepo.GetUsersForReceptionistService((int)UserRole.Guest, parsedGender, filterDto.Keyword, currentPage, pageSize);
        int totalPage = (int)Math.Ceiling((double)totalRecords / pageSize);
        var usersDto = _mapper.Map<List<GetUsersMedicalRecordDto>>(users);
        var resultResponse = new ResponsePagedService<List<GetUsersMedicalRecordDto>>(
            data: usersDto,
            currentPage: currentPage,
            pageSize: pageSize,
            totalPage: totalPage,
            totalItem: totalRecords
        );
        return new ResponseService<ResponsePagedService<List<GetUsersMedicalRecordDto>>>(
            statusCode: (int)HttpStatusCode.OK,
            message: UserMessages.GET_USERS_SUCCESSFULLY,
            data: resultResponse
        );
    }

    // Doctor
    public async Task<ResponseService<ResponsePagedService<List<GetUsersMedicalRecordDto>>>> GetUsersForDoctorService(UserQueryFilterReDto filterDto, PaginationDto paginationDto, Guid currentUserId)
    {
        if (!ValidateAndParsePagination.TryParsePagination<ResponsePagedService<List<GetUsersMedicalRecordDto>>>(paginationDto.Page, paginationDto.PageSize, out int? pageParsed, out int? pageSizeParsed, out var errorResponsePaged))
        {
            return errorResponsePaged!;
        }

        byte? parsedGender = null;
        if (!string.IsNullOrEmpty(filterDto.Gender))
        {
            if (byte.TryParse(filterDto.Gender, out byte genderValue))
            {
                parsedGender = genderValue;
            }
            else
            {
                return new ResponseService<ResponsePagedService<List<GetUsersMedicalRecordDto>>>(
                    statusCode: (int)HttpStatusCode.BadRequest,
                    message: UserMessages.GENDER_IS_INVALID
                );
            }
        }

        int currentPage = pageParsed ?? 1;
        int pageSize = pageSizeParsed ?? 10;
        var userIdsHasAppointmentWithDoctor = appointmentRepo.GetUserIdsHasAppointmentWithDoctor(currentUserId);
        var (users, totalRecords) = await userRepo.GetUsersForDoctorService(userIdsHasAppointmentWithDoctor, parsedGender, filterDto.Keyword, currentPage, pageSize);
        int totalPage = (int)Math.Ceiling((double)totalRecords / pageSize);
        var usersDto = _mapper.Map<List<GetUsersMedicalRecordDto>>(users);
        var resultResponse = new ResponsePagedService<List<GetUsersMedicalRecordDto>>(
            data: usersDto,
            currentPage: currentPage,
            pageSize: pageSize,
            totalPage: totalPage,
            totalItem: totalRecords
        );
        return new ResponseService<ResponsePagedService<List<GetUsersMedicalRecordDto>>>(
            statusCode: (int)HttpStatusCode.OK,
            message: UserMessages.GET_USERS_SUCCESSFULLY,
            data: resultResponse
        );
    }

    #endregion

    #region GET DOCTORS PUBLIC
    public async Task<ResponseService<List<DoctorDto>>> GetDoctorsPublicService()
    {
        var users = await userRepo.GetDoctorsPublicService(u => u.RoleId == (int)UserRole.Doctor);

        var doctors = _mapper.Map<List<DoctorDto>>(users);
        return new ResponseService<List<DoctorDto>>(
            statusCode: (int)HttpStatusCode.OK,
            message: UserMessages.GET_DOCTORS_SUCCESSFULLY,
            data: doctors
        );
    }

    #endregion

    #region GET USER BY ID
    public async Task<ResponseService<GetMeDto>> GetUserByIdService(Guid currentUserId, string userId)
    {
        var currentUser = await userRepo.GetUserWithRoleAsync(u => u.UserId == currentUserId);

        if (currentUser == null)
        {
            return new ResponseService<GetMeDto>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }
        if (!Guid.TryParse(userId, out Guid parsedUserId))
        {
            return new ResponseService<GetMeDto>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: UserMessages.USER_ID_IS_INVALID
            );
        }


        if (currentUserId == parsedUserId)
        {
            return new ResponseService<GetMeDto>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: UserMessages.PLEASE_USE_ROUTE_GET_PROFILE
            );
        }

        // Lấy user theo id cần xem
        var targetUser = await userRepo.GetUserWithRoleAndSpecialtyAndMedicalRecord(u => u.UserId == parsedUserId);

        if (targetUser is null)
        {
            return new ResponseService<GetMeDto>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.USER_NOT_FOUND
            );
        }

        // Kiểm tra theo role
        switch ((UserRole)currentUser.RoleId!)
        {
            case UserRole.Admin:
                // Admin được xem tất cả
                break;

            case UserRole.Receptionist:
                if (targetUser.RoleId != (int)UserRole.Guest && targetUser.RoleId != (int)UserRole.Doctor)
                {
                    return new ResponseService<GetMeDto>(
                        statusCode: (int)HttpStatusCode.BadRequest,
                        message: UserMessages.USER_NOT_FOUND
                    );
                }
                break;

            case UserRole.Doctor:
                if (targetUser.RoleId != (int)UserRole.Guest)
                {
                    return new ResponseService<GetMeDto>(
                        statusCode: (int)HttpStatusCode.BadRequest,
                        message: UserMessages.USER_NOT_FOUND
                    );
                }

                // Kiểm tra xem người dùng có lịch hẹn với bác sĩ này không
                bool hasAppointment = await appointmentRepo.checkHasAppointmentWithCurrentRoleDoctor(parsedUserId, currentUserId);

                if (!hasAppointment)
                {
                    return new ResponseService<GetMeDto>(
                        statusCode: (int)HttpStatusCode.BadRequest,
                        message: UserMessages.USER_NOT_FOUND
                    );
                }
                break;

            default:
                return new ResponseService<GetMeDto>(
                    statusCode: (int)HttpStatusCode.BadRequest,
                    message: UserMessages.USER_NOT_FOUND
                );
        }

        var userDto = _mapper.Map<GetMeDto>(targetUser);
        return new ResponseService<GetMeDto>(
            statusCode: (int)HttpStatusCode.OK,
            message: UserMessages.GET_USER_SUCCESSFULLY,
            data: userDto
        );
    }
    #endregion

    #region GET ME
    public async Task<ResponseService<GetMeDto>> GetMeService(Guid currentUserId)
    {
        var user = await userRepo.GetUserWithRoleAndSpecialtyAndMedicalRecord(u => u.UserId == currentUserId);
        if (user is null)
        {
            return new ResponseService<GetMeDto>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }
        var userDto = _mapper.Map<GetMeDto>(user);
        return new ResponseService<GetMeDto>(
            statusCode: (int)HttpStatusCode.OK,
            message: UserMessages.GET_USER_PROFILE_SUCCESSFULLY,
            data: userDto
        );
    }
    #endregion

    #region UPDATE ME

    public async Task<ResponseService<object>> UpdateMeService(Guid currentUserId, UpdateMeDto dto)
    {
        var errors = new Dictionary<string, string>();
        var user = await userRepo.SingleOrDefaultAsync(u => u.UserId == currentUserId);
        if (user == null)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }

        // Fullname
        CheckAndUpdateString.CheckAndUpdateValueString(errors, "fullname", dto.Fullname, user.Fullname!, UserMessages.FULL_NAME_IS_REQUIRED, val => user.Fullname = val);

        // Gender
        if (dto.Gender.HasValue && dto.Gender != user.Gender)
        {
            user.Gender = dto.Gender.Value;
        }

        // BirthDate
        if (!string.IsNullOrWhiteSpace(dto.BirthDate))
        {
            // Validate Date
            if (!DateOnly.TryParseExact(dto.BirthDate?.ToString(), "yyyy-MM-dd", out DateOnly birthDate))
            {
                errors["birth_date"] = $@"{UserMessages.BIRTH_DATE_IS_INVALID}";
            }
            else
            {
                user.BirthDate = birthDate;
            }
        }

        // Email
        if (dto.Email != null)
        {
            var trimmedValue = dto.Email.Trim();

            if (string.IsNullOrEmpty(trimmedValue))
            {
                errors["email"] = UserMessages.EMAIL_IS_REQUIRED;
            }
            else if (trimmedValue != user.Email)
            {
                var existingUser = await userRepo.SingleOrDefaultAsync(u => u.Email == dto.Email);
                if (existingUser is not null && existingUser.Email != user.Email)
                {
                    errors["email"] = UserMessages.EMAIL_IS_ALREADY_EXIST;
                }
                else
                {
                    user.Email = trimmedValue;
                }
            }
        }

        // Phone
        if (dto.Phone != null)
        {
            var trimmedValue = dto.Phone.Trim();

            if (string.IsNullOrEmpty(trimmedValue))
            {
                errors["phone"] = UserMessages.PHONE_IS_REQUIRED;
            }
            else if (trimmedValue != user.Phone)
            {
                var existingUser = await userRepo.SingleOrDefaultAsync(u => u.Phone == dto.Phone);
                if (existingUser is not null && existingUser.Phone != user.Phone)
                {
                    errors["phone"] = UserMessages.PHONE_IS_ALREADY_EXIST;
                }
                else
                {
                    user.Phone = trimmedValue;
                }
            }
        }

        // Address
        CheckAndUpdateString.CheckAndUpdateValueString(errors, "address", dto.Address, user.Address!, UserMessages.ADDRESS_IS_REQUIRED, val => user.Address = val);

        // Image
        CheckAndUpdateString.CheckAndUpdateValueString(errors, "image", dto.Image, user.Image!, UserMessages.IMAGE_IS_REQUIRED, val => user.Image = val);

        if (errors.Count > 0)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.UnprocessableEntity,
                message: UserMessages.ERROR,
                errors: errors
            );
        }
        user.UpdatedAt = DateTime.Now;
        await userRepo.Update(user);
        await unitOfWork.SaveChangesAsync();

        return new ResponseService<object>(
            statusCode: (int)HttpStatusCode.OK,
            message: UserMessages.UPDATE_PROFILE_SUCCESSFULLY
        );
    }

    #endregion

    #region UPLOAD FILE
    public ResponseService<string> UploadFileService(UploadFileDto dto)
    {
        if (!UploadHandler.UploadFile<string>(dto.File!, out string? urlString, out var errorResponse))
        {
            return errorResponse!;
        }

        return new ResponseService<string>(
            statusCode: (int)HttpStatusCode.OK,
            message: UserMessages.UPLOAD_FILE_SUCCESSFULLY,
            data: urlString
        );
    }
    #endregion

    #region UPDATE USER BY USER ID
    public async Task<ResponseService<object>> UpdateUserService(Guid currentUserId, UpdateUserDto dto, string userId)
    {
        var errors = new Dictionary<string, string>();
        var currentUser = await userRepo.SingleOrDefaultAsync(u => u.UserId == currentUserId);
        if (currentUser == null)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.CURRENT_USER_NOT_FOUND
            );
        }

        if (!Guid.TryParse(userId, out Guid parsedUserId))
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: UserMessages.USER_ID_IS_INVALID
            );
        }

        if (currentUserId == parsedUserId)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: UserMessages.PLEASE_USE_ROUTE_UPDATE_ME
            );
        }

        var user = await userRepo.GetUserWithRoleAndSpecialtyAndMedicalRecord(u => u.UserId == parsedUserId);
        if (user == null)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.USER_NOT_FOUND
            );
        }

        // Validate quyền chỉnh sửa dựa theo role
        var isAdmin = currentUser.RoleId == (int)UserRole.Admin;
        var isReceptionist = currentUser.RoleId == (int)UserRole.Receptionist;

        if (isReceptionist)
        {
            // Receptionist chỉ được chỉnh sửa Guest
            if (user.RoleId != (int)UserRole.Guest)
            {
                return new ResponseService<object>(
                    statusCode: (int)HttpStatusCode.NotFound,
                    message: UserMessages.USER_NOT_FOUND
                );
            }
        }
        // SpecialtyId chỉ được Admin 
        // if (isAdmin)
        // {
        //     if (user.RoleId == (int)UserRole.Doctor && isAdmin)
        //     {
        //         if (!string.IsNullOrWhiteSpace(dto.SpecialtyId))
        //         {
        //             if (!isAdmin)
        //             {
        //                 errors["specialty_id"] = UserMessages.PERMISSION_DENIED;
        //             }
        //             else if (!int.TryParse(dto.SpecialtyId, out int parsedSpecialtyId))
        //             {
        //                 errors["specialty_id"] = UserMessages.SPECIALTY_ID_IS_INVALID;
        //             }
        //             else
        //             {
        //                 user.SpecialtyId = parsedSpecialtyId;
        //             }
        //         }
        //         return new ResponseService<object>(
        //             statusCode: (int)HttpStatusCode.NotFound,
        //             message: UserMessages.USER_NOT_FOUND
        //         );
        //     }

        // }


        // FullName
        CheckAndUpdateString.CheckAndUpdateValueString(errors, "fullname", dto.Fullname, user.Fullname!, UserMessages.FULL_NAME_IS_REQUIRED, val => user.Fullname = val);

        // Weight
        CheckAndUpdateString.CheckAndUpdateValueString(errors, "weight", dto.Weight, user.Weight.ToString()!, UserMessages.WEIGHT_IS_REQUIRED, (val) =>
        {
            if (double.TryParse(val, out double parsedWeight))
            {
                user.Weight = parsedWeight;
            }
            else
            {
                errors["weight"] = UserMessages.WEIGHT_MUST_BE_A_NUMBER;
            }
        });

        // Gender
        if (dto.Gender.HasValue && dto.Gender != user.Gender)
        {
            user.Gender = dto.Gender.Value;
        }

        // BirthDate
        if (dto.BirthDate != null)
        {
            var trimmedValue = dto.BirthDate.Trim();
            if (string.IsNullOrEmpty(trimmedValue))
            {
                errors["birth_date"] = UserMessages.BIRTH_DATE_IS_REQUIRED;

            }
            else if (!DateOnly.TryParseExact(dto.BirthDate, "yyyy-MM-dd", out var birthDate))
            {
                errors["birth_date"] = UserMessages.BIRTH_DATE_IS_INVALID;
            }
            else
            {
                user.BirthDate = birthDate;
            }
        }
        // Email
        if (dto.Email != null)
        {
            var trimmed = dto.Email.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                errors["email"] = UserMessages.EMAIL_IS_REQUIRED;
            }
            else if (trimmed != user.Email)
            {
                var exist = await userRepo.SingleOrDefaultAsync(u => u.Email == trimmed && u.UserId != user.UserId);
                if (exist != null)
                {
                    errors["email"] = UserMessages.EMAIL_IS_ALREADY_EXIST;
                }
                else
                {
                    user.Email = trimmed;
                }
            }
        }

        // Phone
        if (dto.Phone != null)
        {
            var trimmed = dto.Phone.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                errors["phone"] = UserMessages.PHONE_IS_REQUIRED;
            }
            else if (trimmed != user.Phone)
            {
                var exist = await userRepo.SingleOrDefaultAsync(u => u.Phone == trimmed && u.UserId != user.UserId);
                if (exist != null)
                {
                    errors["phone"] = UserMessages.PHONE_IS_ALREADY_EXIST;
                }
                else
                {
                    user.Phone = trimmed;
                }
            }
        }

        // Address
        CheckAndUpdateString.CheckAndUpdateValueString(errors, "address", dto.Address, user.Address!, UserMessages.ADDRESS_IS_REQUIRED, val => user.Address = val);

        // Image
        CheckAndUpdateString.CheckAndUpdateValueString(errors, "image", dto.Image, user.Image!, UserMessages.IMAGE_IS_REQUIRED, val => user.Image = val);



        if (errors.Count > 0)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.UnprocessableEntity,
                message: UserMessages.ERROR,
                errors: errors
            );
        }

        user.UpdatedAt = DateTime.Now;
        await userRepo.Update(user);
        await unitOfWork.SaveChangesAsync();

        return new ResponseService<object>(
            statusCode: (int)HttpStatusCode.OK,
            message: UserMessages.UPDATE_USER_SUCCESSFULLY
        );
    }

    #endregion

    #region ADD USER
    public async Task<ResponseService<object>> AddUserForAdminService(AddUserDto dto)
    {
        var errors = new Dictionary<string, string>();
        int? parsedRoleId = null;
        int? parsedSpecialtyId = null;

        // Validate Role (Enum)
        if (string.IsNullOrWhiteSpace(dto.RoleId) || !int.TryParse(dto.RoleId, out int roleId))
        {
            errors["role_id"] = $@"{UserMessages.ROLE_MUST_BE_A_NUMBER}";
        }
        else if (!EnumValidationService.IsValid<UserRole>(roleId))
        {
            errors["role_id"] = $@"{UserMessages.ROLE_IS_INVALID}: {EnumValidationService.GetValidEnumValues<UserRole>()}";
        }
        else
        {
            parsedRoleId = roleId;
        }

        // Validate Date
        if (!DateOnly.TryParseExact(dto.BirthDate?.ToString(), "yyyy-MM-dd", out DateOnly birthDate))
        {
            errors["birth_date"] = $@"{UserMessages.BIRTH_DATE_IS_INVALID}";
        }
        bool isDoctor = parsedRoleId == (int)UserRole.Doctor;
        bool isReceptionist = parsedRoleId == (int)UserRole.Receptionist;
        bool isTechnician = parsedRoleId == (int)UserRole.Technician;

        if (isDoctor)
        {
            if (string.IsNullOrWhiteSpace(dto.SpecialtyId))
            {
                errors["specialty_id"] = UserMessages.ROLE_DOCTOR_MUST_HAVE_SPECIALTY;
            }
            else
            {
                if (!ValidateAndParseSpecialty.TryParseSpecialty<object>(dto.SpecialtyId, out parsedSpecialtyId, out var errorResponseSpecialty, isFromBody: true))
                {
                    return errorResponseSpecialty!;
                }
                if (parsedSpecialtyId.HasValue)
                {
                    if (!EnumValidationService.IsValid<SpecialtyEnum>(parsedSpecialtyId.Value))
                    {
                        errors["specialty_id"] = $@"{UserMessages.SPECIALTY_IS_INVALID}: {EnumValidationService.GetValidEnumValues<SpecialtyEnum>()}";
                    }
                }
            }

        }
        else
        {
            if (!string.IsNullOrWhiteSpace(dto.SpecialtyId))
            {
                errors["specialty_id"] = UserMessages.ONLY_ROLE_DOCTOR_HAS_SPECIALTY;
            }
        }

        if ((isDoctor || isReceptionist || isTechnician) && string.IsNullOrWhiteSpace(dto.Email))
            errors["email"] = UserMessages.EMAIL_IS_REQUIRED;

        if ((isDoctor || isReceptionist || isTechnician) && string.IsNullOrWhiteSpace(dto.Image))
            errors["image"] = UserMessages.IMAGE_IS_REQUIRED;

        // Check Email
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var emailExists = await userRepo.SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (emailExists is not null)
                errors["email"] = UserMessages.EMAIL_IS_ALREADY_EXIST;
        }

        // Check Phone
        if (!string.IsNullOrWhiteSpace(dto.Phone))
        {
            var phoneExists = await userRepo.SingleOrDefaultAsync(u => u.Phone == dto.Phone);
            if (phoneExists is not null)
                errors["phone"] = UserMessages.PHONE_IS_ALREADY_EXIST;
        }


        if (errors.Count > 0)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.UnprocessableEntity,
                message: UserMessages.ERROR,
                data: errors
            );
        }

        var user = new User()
        {
            Fullname = dto.Fullname,
            Email = dto.Email,
            Password = new PasswordHasher<User>().HashPassword(new User(), "abc"),
            Address = dto.Address,
            Gender = dto.Gender,
            BirthDate = birthDate,
            Image = dto.Image,
            Phone = dto.Phone,
            RoleId = parsedRoleId,
            SpecialtyId = parsedSpecialtyId,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        await userRepo.AddAsync(user);
        await unitOfWork.SaveChangesAsync();

        return new ResponseService<object>(
            statusCode: (int)HttpStatusCode.OK,
            message: UserMessages.ADD_USER_SUCCESSFULLY
        );

    }


    public async Task<ResponseService<object>> AddUserForReceptionistService(AddUserDto dto)
    {
        var errors = new Dictionary<string, string>();
        int? parsedRoleId = null;

        // Validate Role (Enum)
        if (string.IsNullOrWhiteSpace(dto.RoleId) || !int.TryParse(dto.RoleId, out int roleId))
        {
            errors["role_id"] = $@"{UserMessages.ROLE_MUST_BE_A_NUMBER}";
        }
        else if (roleId != (int)UserRole.Guest)
        {
            errors["role_id"] = $@"{UserMessages.ROLE_MUST_BE_A_GUEST}";
        }
        else
        {
            parsedRoleId = roleId;
        }

        // Validate Date
        if (!DateOnly.TryParseExact(dto.BirthDate?.ToString(), "yyyy-MM-dd", out DateOnly birthDate))
        {
            errors["birth_date"] = $@"{UserMessages.BIRTH_DATE_IS_INVALID}";
        }

        // Check Email
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var emailExists = await userRepo.SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (emailExists is not null)
                errors["email"] = UserMessages.EMAIL_IS_ALREADY_EXIST;
        }

        // Check Phone
        if (!string.IsNullOrWhiteSpace(dto.Phone))
        {
            var phoneExists = await userRepo.SingleOrDefaultAsync(u => u.Phone == dto.Phone);
            if (phoneExists is not null)
                errors["phone"] = UserMessages.PHONE_IS_ALREADY_EXIST;
        }

        if (errors.Count > 0)
        {
            return new ResponseService<object>(
                statusCode: (int)HttpStatusCode.UnprocessableEntity,
                message: UserMessages.ERROR,
                errors: errors
            );
        }

        var user = new User()
        {
            Fullname = dto.Fullname,
            Email = dto.Email,
            Password = new PasswordHasher<User>().HashPassword(new User(), "abc"),
            Address = dto.Address,
            Gender = dto.Gender,
            BirthDate = birthDate,
            Image = dto.Image,
            Phone = dto.Phone,
            RoleId = parsedRoleId,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        await userRepo.AddAsync(user);
        await unitOfWork.SaveChangesAsync();

        return new ResponseService<object>(
            statusCode: (int)HttpStatusCode.Created,
            message: UserMessages.ADD_USER_SUCCESSFULLY
        );
    }




    #endregion


    #region GET ALL RECETIONIST FOR CHATTING
    public async Task<ResponseService<List<GetUsersDto>>> GetReceptionistsService()
    {
        var users = await userRepo.GetUsersByRoleService((int)UserRole.Receptionist);
        var receptionists = _mapper.Map<List<GetUsersDto>>(users);
        return new ResponseService<List<GetUsersDto>>(
            statusCode: (int)HttpStatusCode.OK,
            message: UserMessages.GET_RECEPTIONISTS_SUCCESSFULLY,
            data: receptionists
        );
    }

    #endregion

    #region GET ALL DOCTORS FOR CHATTING
    public async Task<ResponseService<List<GetUsersDto>>> GetAllDoctors()
    {
        var users = await userRepo.GetUsersByRoleService((int)UserRole.Doctor);
        var doctors = _mapper.Map<List<GetUsersDto>>(users);
        return new ResponseService<List<GetUsersDto>>(
            statusCode: (int)HttpStatusCode.OK,
            message: UserMessages.GET_DOCTORS_SUCCESSFULLY,
            data: doctors
        );
    }
    #endregion
}