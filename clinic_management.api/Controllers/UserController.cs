namespace clinic_management.api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    #region LOGIN
    [HttpPost("login")]
    public async Task<ActionResult<ResponseService<TokenResponse>>> Login([FromBody] UserLoginDto userLoginDto)
    {
        var result = await userService.LoginService(userLoginDto);
        return result.StatusCode switch
        {
            400 => BadRequest(result),
            404 => NotFound(result),
            422 => UnprocessableEntity(result),
            _ => Ok(result)
        };
    }
    #endregion


    #region REFRESH TOKEN
    [Authorize]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<ResponseService<TokenResponse>>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        Guid userId = UtilCommon.GetUserIdFromHeader(User);
        var result = await userService.RefreshTokenService(userId, refreshTokenDto);
        return result!.StatusCode switch
        {
            400 => BadRequest(result),
            422 => UnprocessableEntity(result),
            _ => Ok(result)
        };
    }
    #endregion

    #region LOGOUT
    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ResponseService<string>>> Logout([FromBody] RefreshTokenDto refreshTokenDto)
    {
        Guid userId = UtilCommon.GetUserIdFromHeader(User);
        var result = await userService.LogoutService(userId, refreshTokenDto);
        return result!.StatusCode switch
        {
            400 => BadRequest(result),
            404 => NotFound(result),
            422 => UnprocessableEntity(result),
            _ => Ok(result)
        };
    }
    #endregion

    #region GET USERS

    [HttpGet("get-users-for-admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseService<ResponsePagedService<List<GetUsersAdminDto>>>>> GetUsers([FromQuery] UserQueryFilterDto filterDto, [FromQuery] PaginationDto paginationDto)
    {
        var result = await userService.GetUsersForAdminService(filterDto, paginationDto);
        return result!.StatusCode switch
        {
            400 => BadRequest(result),
            422 => UnprocessableEntity(result),
            _ => Ok(result)
        };
    }

    [HttpGet("get-users-for-receptionist")]
    [Authorize(Roles = "Receptionist")]
    public async Task<ActionResult<ResponseService<ResponsePagedService<List<GetUsersMedicalRecordDto>>>>> GetUsersForReceptionist([FromQuery] UserQueryFilterReDto userQueryFilterDto, [FromQuery] PaginationDto paginationDto)
    {
        var result = await userService.GetUsersForReceptionistService(userQueryFilterDto, paginationDto);
        return result!.StatusCode switch
        {
            400 => BadRequest(result),
            422 => UnprocessableEntity(result),
            _ => Ok(result)
        };
    }

    [HttpGet("get-users-for-doctor")]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<ResponseService<ResponsePagedService<List<GetUsersMedicalRecordDto>>>>> GetUsersForDoctorService([FromQuery] UserQueryFilterReDto userQueryFilterDto, [FromQuery] PaginationDto paginationDto)
    {
        Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
        var result = await userService.GetUsersForDoctorService(userQueryFilterDto, paginationDto, currentUserId);
        return result!.StatusCode switch
        {
            400 => BadRequest(result),
            422 => UnprocessableEntity(result),
            _ => Ok(result)
        };
    }
    #endregion


    #region GET DOCTORS PUBLIC
    [HttpGet("get-doctors")]
    public async Task<ActionResult<ResponseService<List<DoctorDto>>>> GetDoctorsPublic()
    {
        var result = await userService.GetDoctorsPublicService();
        return result!.StatusCode switch
        {
            400 => BadRequest(result),
            422 => UnprocessableEntity(result),
            _ => Ok(result)
        };
    }
    #endregion

    #region GET DOCTORS 2
    [HttpGet("get-all-doctors")]
    public async Task<ActionResult<ResponseService<List<GetUsersDto>>>> GetAllDoctors()
    {
        var result = await userService.GetAllDoctors();
        return result!.StatusCode switch
        {
            400 => BadRequest(result),
            422 => UnprocessableEntity(result),
            _ => Ok(result)
        };
    }
    #endregion


    #region GET RECEPTIONISTS
    [HttpGet("get-receptionists")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<ActionResult<ResponseService<List<GetUsersDto>>>> GetReceptionists()
    {
        var result = await userService.GetReceptionistsService();
        return result!.StatusCode switch
        {
            400 => BadRequest(result),
            422 => UnprocessableEntity(result),
            _ => Ok(result)
        };
    }
    #endregion

    #region GET USER BY ID
    [HttpGet("get-user/{userId}")]
    [Authorize(Roles = "Admin,Doctor,Receptionist")]
    public async Task<ActionResult<ResponseService<GetMeDto>>> GetUserById([FromRoute] string userId)
    {
        Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
        var result = await userService.GetUserByIdService(currentUserId, userId);
        return result!.StatusCode switch
        {
            400 => BadRequest(result),
            404 => NotFound(result),
            422 => UnprocessableEntity(result),
            _ => Ok(result)
        };
    }
    #endregion

    #region GET ME

    [HttpGet("get-me")]
    [Authorize]
    public async Task<ActionResult<ResponseService<GetMeDto>>> GetMe()
    {
        Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
        var result = await userService.GetMeService(currentUserId);
        return result!.StatusCode switch
        {
            400 => BadRequest(result),
            404 => NotFound(result),
            422 => UnprocessableEntity(result),
            _ => Ok(result)
        };
    }
    #endregion


    #region UPLOAD FILE
    [HttpPost("upload-file")]
    public ActionResult<ResponseService<string>> UploadFile([FromForm] UploadFileDto dto)
    {
        var result = userService.UploadFileService(dto);
        return result!.StatusCode switch
        {
            400 => BadRequest(result),
            422 => UnprocessableEntity(result),
            _ => Ok(result)
        };
    }
    #endregion

    #region UPDATE ME
    [HttpPatch("update-me")]
    [Authorize]
    public async Task<ActionResult<ResponseService<object>>> UpdateMe([FromBody] UpdateMeDto dto)
    {
        Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
        var result = await userService.UpdateMeService(currentUserId, dto);
        return result!.StatusCode switch
        {
            400 => BadRequest(result),
            404 => NotFound(result),
            422 => UnprocessableEntity(result),
            _ => Ok(result)
        };
    }
    #endregion


    #region UPDATE USER BY USER ID
    [HttpPatch("update-user/{userId}")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<ActionResult<ResponseService<object>>> UpdateUser([FromBody] UpdateUserDto dto, [FromRoute] string userId)
    {
        Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
        var result = await userService.UpdateUserService(currentUserId, dto, userId);
        return result!.StatusCode switch
        {
            400 => BadRequest(result),
            404 => NotFound(result),
            422 => UnprocessableEntity(result),
            _ => Ok(result)
        };
    }

    #endregion


    #region ADD USER
    [HttpPost("add-user-for-admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseService<object>>> AddUserForAdmin([FromBody] AddUserDto dto)
    {
        var result = await userService.AddUserForAdminService(dto);
        return result!.StatusCode switch
        {
            400 => BadRequest(result),
            422 => UnprocessableEntity(result),
            _ => Ok(result)
        };
    }

    [HttpPost("add-user-for-receptionist")]
    [Authorize(Roles = "Receptionist")]
    public async Task<ActionResult<ResponseService<object>>> AddUserForReceptionist([FromBody] AddUserDto dto)
    {
        var result = await userService.AddUserForReceptionistService(dto);
        return result!.StatusCode switch
        {
            400 => BadRequest(result),
            422 => UnprocessableEntity(result),
            _ => Ok(result)
        };
    }
    #endregion


}