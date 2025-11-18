using System.Net;
using AutoMapper;
using clinic_management.infrastructure.Models;

public interface IChattingService
{
    public Task<ResponseService<ResponsePagedService<List<GetMessageDto>>>> GetMessagesByConvIdService(Guid currentUserId, string conversationId, PaginationDto dto);
    public Task<ResponseService<GetConversationDto>> GetOrCreateConversationService(Guid currentUserId, GetOrCreateConversationDto dto);
    public Task<ResponseService<GetMessageDto>> SendMessageService(Guid currentUserId, SendMessageDto dto);


}

public class ChattingService(IMapper _mapper, IUnitOfWork unitOfWork, IConversationRepository conversationRepo, IMessageRepository messageRepo, IUserRepository userRepo) : IChattingService
{
    #region GET CHATTING
    public async Task<ResponseService<ResponsePagedService<List<GetMessageDto>>>> GetMessagesByConvIdService(Guid currentUserId, string conversationId, PaginationDto dto)
    {
        if (!int.TryParse(conversationId, out int convId))
        {
            return new ResponseService<ResponsePagedService<List<GetMessageDto>>>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: ChattingMessages.CONVERSATION_ID_MUST_BE_A_NUMBER
            );
        }

        var conversation = await conversationRepo.GetConversationByConvId(convId);
        if (conversation is null)
        {
            return new ResponseService<ResponsePagedService<List<GetMessageDto>>>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: ChattingMessages.CONVERSATION_NOT_FOUND
            );
        }

        // Check user có trong conversation
        if (conversation.User1Id != currentUserId && conversation.User2Id != currentUserId)
        {
            return new ResponseService<ResponsePagedService<List<GetMessageDto>>>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: ChattingMessages.USER_DO_NOT_PART_IN_THIS_CONVERSATION
            );
        }

        if (!ValidateAndParsePagination.TryParsePagination<ResponsePagedService<List<GetMessageDto>>>(dto.Page, dto.PageSize, out int? pageParsed, out int? pageSizeParsed, out var errorResponsePaged))
        {
            return errorResponsePaged!;
        }

        int currentPage = pageParsed ?? 1;
        int pageSize = pageSizeParsed ?? 10;

        var (lstMessages, totalRecords) = await messageRepo.GetMessagesByConvId(convId, currentPage, pageSize);
        int totalPage = (int)Math.Ceiling((double)totalRecords / pageSize);

        var lstMessagesMapper = _mapper.Map<List<GetMessageDto>>(lstMessages);
        var resultResponse = new ResponsePagedService<List<GetMessageDto>>(
           data: lstMessagesMapper,
           currentPage: currentPage,
           pageSize: pageSize,
           totalPage: totalPage,
           totalItem: totalRecords
       );

        return new ResponseService<ResponsePagedService<List<GetMessageDto>>>(
            statusCode: (int)HttpStatusCode.OK,
            message: ChattingMessages.GET_MESSAGES_SUCCESSFULLY,
            data: resultResponse
        );
    }
    #endregion

    #region GET OR CREATE CONVERSATION
    public async Task<ResponseService<GetConversationDto>> GetOrCreateConversationService(Guid currentUserId, GetOrCreateConversationDto dto)
    {
        // Validate User Id
        if (!Guid.TryParse(dto.User2Id, out var parsedUserId2))
        {
            return new ResponseService<GetConversationDto>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: ChattingMessages.USER_ID_MUST_BE_GUID
            );
        }

        if (currentUserId == parsedUserId2)
        {
            return new ResponseService<GetConversationDto>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: ChattingMessages.CANNOT_CREATE_CONVERSATION_WITH_YOURSELF
            );
        }

        var receiver = await userRepo.GetUserWithRoleAsync(u => u.UserId == parsedUserId2);
        if (receiver is null)
        {
            return new ResponseService<GetConversationDto>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: UserMessages.USER_NOT_FOUND
            );
        }

        // chuẩn hóa cặp user
        var minId = currentUserId.CompareTo(parsedUserId2) < 0 ? currentUserId : parsedUserId2;
        var maxId = currentUserId.CompareTo(parsedUserId2) < 0 ? parsedUserId2 : currentUserId;

        var conversation = await conversationRepo.GetConversationOfTwoUserId(minId, maxId);
        if (conversation is not null)
        {
            var conversationMapper = _mapper.Map<GetConversationDto>(conversation);
            return new ResponseService<GetConversationDto>(
                statusCode: (int)HttpStatusCode.OK,
                message: ChattingMessages.GET_CONVERSATION_SUCCESSFULLY,
                data: conversationMapper
            );
        }

        // nếu chưa có thì tạo mới
        var newConv = new Conversation
        {
            User1Id = currentUserId,
            User2Id = parsedUserId2,
            CreatedAt = DateTime.UtcNow
        };
        await conversationRepo.AddAsync(newConv);

        await unitOfWork.SaveChangesAsync();
        var createAndGetConversationMapper = _mapper.Map<GetConversationDto>(newConv);

        return new ResponseService<GetConversationDto>(
            statusCode: (int)HttpStatusCode.OK,
            message: ChattingMessages.CREATE_AND_GET_CONVERSATION_SUCCESSFULLY,
            data: createAndGetConversationMapper
        );
    }
    #endregion

    #region SEND MESSAGE
    public async Task<ResponseService<GetMessageDto>> SendMessageService(Guid currentUserId, SendMessageDto dto)
    {
        // lấy conversation
        if (!int.TryParse(dto.ConversationId, out int convId))
        {
            return new ResponseService<GetMessageDto>(
                statusCode: (int)HttpStatusCode.BadRequest,
                message: ChattingMessages.CONVERSATION_ID_MUST_BE_A_NUMBER
            );
        }

        var conversation = await conversationRepo.GetConversationByConvId(convId);
        if (conversation is null)
        {
            return new ResponseService<GetMessageDto>(
                statusCode: (int)HttpStatusCode.NotFound,
                message: ChattingMessages.CONVERSATION_NOT_FOUND
            );
        }
        var message = new Message
        {
            Conversation = conversation,
            SenderId = currentUserId,
            Content = dto.Content!,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await messageRepo.AddAsync(message);
        conversation.LastMessage = message;

        await conversationRepo.Update(conversation);
        await unitOfWork.SaveChangesAsync();
        var mapperMessage = _mapper.Map<GetMessageDto>(message);

        return new ResponseService<GetMessageDto>(
            statusCode: (int)HttpStatusCode.OK,
            message: ChattingMessages.SEND_MESSAGE_SUCCESSFULLY,
            data: mapperMessage
        );
    }

    #endregion


}