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
        if (!currentUserId.HasValue)
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized"
            };

        var existedConversation =
            await _unitOfWork.ConversationRepository.FindByAccountIdAndConversationIdAsync(currentUserId.Value,
                messageAddModel.ConversationId);
        if (existedConversation == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status403Forbidden,
                Message = "Cannot access the conversation"
            };

        var message = _mapper.Map<Message>(messageAddModel);
        foreach (var accountConversation in existedConversation.AccountConversations)
            message.MessageRecipients.Add(new MessageRecipient
            {
                IsRead = accountConversation.AccountId == currentUserId,
                AccountId = accountConversation.AccountId,
                AccountConversationId = accountConversation.Id
            });

        await _unitOfWork.MessageRepository.AddAsync(message);
        if (await _unitOfWork.SaveChangeAsync() > 0)
            return new ResponseModel
            {
                Code = StatusCodes.Status201Created,
                Message = "Create message successfully"
            };

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot create message"
        };
    }
}