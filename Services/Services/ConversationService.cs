using AutoMapper;
using Microsoft.AspNetCore.Http;
using Repositories.Entities;
using Repositories.Interfaces;
using Repositories.Models.ConversationModels;
using Repositories.Models.MessageModels;
using Services.Interfaces;
using Services.Models.ConversationModels;
using Services.Models.ResponseModels;

namespace Services.Services;

public class ConversationService : IConversationService
{
    private readonly IClaimService _claimService;
    private readonly IMapper _mapper;
    private readonly IRedisHelper _redisHelper;
    private readonly IUnitOfWork _unitOfWork;

    public ConversationService(IClaimService claimService, IMapper mapper, IRedisHelper redisHelper,
        IUnitOfWork unitOfWork)
    {
        _claimService = claimService;
        _mapper = mapper;
        _redisHelper = redisHelper;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseModel> Add(ConversationAddModel conversationAddModel)
    {
        var currentUserId = _claimService.GetCurrentUserId;
        if (!currentUserId.HasValue)
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized"
            };

        var existedConversation =
            await _unitOfWork.ConversationRepository.FindByAccountIdsAsync([
                currentUserId.Value, conversationAddModel.RecipientId
            ]);
        if (existedConversation != null)
            return new ResponseModel
            {
                Code = StatusCodes.Status409Conflict,
                Message = "Conversation already exists"
            };

        var recipientAccount = await _unitOfWork.AccountRepository.GetAsync(conversationAddModel.RecipientId);
        if (recipientAccount == null || recipientAccount.IsDeleted)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Recipient account not found"
            };

        var conversation = new Conversation
        {
            AccountConversations = new List<AccountConversation>([
                new AccountConversation
                {
                    IsOwner = true,
                    AccountId = currentUserId.Value
                },
                new AccountConversation
                {
                    Account = recipientAccount
                }
            ])
        };
        await _unitOfWork.ConversationRepository.AddAsync(conversation);
        if (await _unitOfWork.SaveChangeAsync() > 0)
            return new ResponseModel
            {
                Code = StatusCodes.Status201Created,
                Message = "Create conversation successfully"
            };

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot create conversation"
        };
    }

    public async Task<ResponseModel> Get(Guid id)
    {
        var currentUserId = _claimService.GetCurrentUserId;
        if (!currentUserId.HasValue)
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized"
            };

        var cacheKey = $"conversation_{id}_account_{currentUserId}";
        var responseModel = await _redisHelper.GetOrSetAsync(cacheKey, async () =>
        {
            var conversation =
                await _unitOfWork.ConversationRepository.FindByAccountIdAndConversationIdAsync(currentUserId.Value, id);
            if (conversation == null)
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Conversation not found"
                };

            var senderAccountConversations =
                conversation.AccountConversations.First(accountConversation =>
                    accountConversation.AccountId == currentUserId);
            var recipientAccountConversations =
                conversation.AccountConversations.First(accountConversation =>
                    accountConversation.AccountId != currentUserId);
            var latestMessage = senderAccountConversations.MessageRecipients
                .Select(messageRecipient => messageRecipient.Message).FirstOrDefault();
            var conversationModel = new ConversationModel
            {
                Id = conversation.Id,
                CreationDate = conversation.CreationDate,
                IsDeleted = conversation.IsDeleted,
                Name = recipientAccountConversations.Account.FirstName,
                Image = recipientAccountConversations.Account.Image,
                IsRestricted = conversation.IsRestricted,
                NumberOfUnreadMessages = senderAccountConversations!.MessageRecipients.Count(messageRecipient =>
                    !messageRecipient.IsRead && !messageRecipient.IsDeleted),
                IsArchived = senderAccountConversations.IsArchived,
                IsOwner = senderAccountConversations.IsOwner,
                LatestMessage = latestMessage == null ? null : _mapper.Map<MessageModel>(latestMessage)
            };

            return new ResponseModel
            {
                Message = "Get conversation successfully",
                Data = conversationModel
            };
        });

        return responseModel;
    }

    public async Task<ResponseModel> GetAll(ConversationFilterModel conversationFilterModel)
    {
        return new ResponseModel();
    }
}