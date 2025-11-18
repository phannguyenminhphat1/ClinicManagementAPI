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
    public class ChattingController(IChattingService chattingService) : ControllerBase
    {
        [HttpGet("get-messages/{conversationId}")]
        [Authorize(Roles = "Doctor,Receptionist,Admin")]
        public async Task<ActionResult<ResponseService<ResponsePagedService<List<GetMessageDto>>>>> GetMessagesByConvId(string conversationId, PaginationDto dto)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await chattingService.GetMessagesByConvIdService(currentUserId, conversationId, dto);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }

        [HttpPost("get-or-create-conversation")]
        [Authorize(Roles = "Doctor,Receptionist,Admin")]
        public async Task<ActionResult<ResponseService<GetConversationDto>>> GetOrCreateConversation([FromBody] GetOrCreateConversationDto dto)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await chattingService.GetOrCreateConversationService(currentUserId, dto);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }

        [HttpPost("send-message")]
        [Authorize(Roles = "Doctor,Receptionist,Admin")]
        public async Task<ActionResult<ResponseService<GetMessageDto>>> SendMessage([FromBody] SendMessageDto dto)
        {
            Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
            var result = await chattingService.SendMessageService(currentUserId, dto);
            return result!.StatusCode switch
            {
                400 => BadRequest(result),
                404 => NotFound(result),
                422 => UnprocessableEntity(result),
                _ => Ok(result)
            };
        }

        // [HttpGet("get-user-chatting")]
        // [Authorize(Roles = "Doctor,Receptionist")]
        // public async Task<ActionResult<ResponseService<List<UserChattingDto>>>> GetUserChatting()
        // {
        //     Guid currentUserId = UtilCommon.GetUserIdFromHeader(User);
        //     var result = await chattingService.GetUserChattingService(currentUserId);
        //     return result!.StatusCode switch
        //     {
        //         400 => BadRequest(result),
        //         404 => NotFound(result),
        //         422 => UnprocessableEntity(result),
        //         _ => Ok(result)
        //     };
        // }

    }
}