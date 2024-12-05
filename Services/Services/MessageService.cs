using AutoMapper;
using Microsoft.AspNetCore.Http;
using Repositories.Entities;
using Repositories.Interfaces;
using Services.Interfaces;
using Services.Models.MessageModels;
using Services.Models.ResponseModels;

namespace Services.Services;

public class MessageService : IMessageService
{
    private readonly IClaimService _claimService;
    private readonly IMapper _mapper;
    private readonly IRedisHelper _redisHelper;
    private readonly IUnitOfWork _unitOfWork;

    public MessageService(IClaimService claimService, IMapper mapper, IRedisHelper redisHelper, IUnitOfWork unitOfWork)
    {
        _claimService = claimService;
        _mapper = mapper;
        _redisHelper = redisHelper;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseModel> Add(MessageAddModel messageAddModel)
    {
        var currentUserId = _claimService.GetCurrentUserId;
        if (currentUserId == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized"
            };

        var accountConversation =
            await _unitOfWork.AccountConversationRepository.FindByAccountIdAndConversationIdAsync(
                currentUserId.Value, messageAddModel.ConversationId);
        if (accountConversation == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Conversation not found"
            };

        var members =
            await _unitOfWork.AccountConversationRepository.GetAllActiveMembersByConversationIdAsync(messageAddModel
                .ConversationId);
        var message = _mapper.Map<Message>(messageAddModel);
        message.AttachmentUrl = messageAddModel.Body;
        var messageRecipients = new List<MessageRecipient>();
        foreach (var member in members)
        {
            messageRecipients.Add(new MessageRecipient
            {
                AccountId = member.Id,
                Message = message,
                AccountConversation =
                    member.AccountConversations.FirstOrDefault(ac => ac.ConversationId == messageAddModel.ConversationId)!,
            });
        }
        await _unitOfWork.MessageRecipientRepository.AddRangeAsync(messageRecipients);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            return new ResponseModel
            {
                Code = StatusCodes.Status201Created,
                Message = "Create message successfully"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot create message"
        };
    }
}