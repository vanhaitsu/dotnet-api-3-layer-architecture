using AutoMapper;
using Microsoft.AspNetCore.Http;
using Repositories.Entities;
using Repositories.Interfaces;
using Repositories.Models.AccountConversationModels;
using Repositories.Models.ConversationModels;
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
        if (currentUserId == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized"
            };

        var accountIdWithCurrentUserId = new List<Guid>(conversationAddModel.AccountIds)
        {
            currentUserId.Value
        };
        var validAccountIds = await _unitOfWork.AccountRepository.GetValidAccountIdsAsync(accountIdWithCurrentUserId);
        if (validAccountIds.Count <= 1)
            return new ResponseModel
            {
                Code = StatusCodes.Status400BadRequest,
                Message = "No account ids provided"
            };

        var existedConversation = await _unitOfWork.ConversationRepository.FindByAccountIdsAsync(validAccountIds);
        if (existedConversation != null)
            return new ResponseModel
            {
                Code = StatusCodes.Status409Conflict,
                Message = "Conversation already exists"
            };

        var conversation = _mapper.Map<Conversation>(conversationAddModel);
        var accountConversations = new List<AccountConversation>();
        foreach (var accountId in validAccountIds)
        {
            accountConversations.Add(new AccountConversation
            {
                IsOwner = accountId == currentUserId,
                AccountId = accountId,
                Conversation = conversation
            });
        }

        await _unitOfWork.AccountConversationRepository.AddRangeAsync(accountConversations);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            return new ResponseModel
            {
                Message = "Create conversation successfully"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot create conversation"
        };
    }

    public async Task<ResponseModel> Get(Guid id)
    {
        var currentUserId = _claimService.GetCurrentUserId;
        if (currentUserId == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized"
            };

        var cacheKey = $"conversation_{id}";
        var responseModel = await _redisHelper.GetOrSetAsync(cacheKey, async () =>
        {
            var conversation = await _unitOfWork.ConversationRepository.GetAsync(id, "AccountConversations.Account");
            if (conversation == null)
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Conversation not found"
                };

            var existedAccountConversation =
                conversation.AccountConversations.FirstOrDefault(accountConversation =>
                    accountConversation.AccountId == currentUserId && !accountConversation.IsDeleted);
            if (existedAccountConversation == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status403Forbidden,
                    Message = "Cannot get conversation"
                };
            }

            var conversationModel = _mapper.Map<ConversationModel>(conversation);
            conversationModel.IsGroup = conversationModel.NumberOfMembers > 2;
            if (!conversationModel.IsGroup)
            {
                var recipientAccountConversation = conversation.AccountConversations.FirstOrDefault(
                    accountConversation =>
                        accountConversation.Account.Id != currentUserId)!;
                conversationModel.Name =
                    $"{recipientAccountConversation.Account.FirstName} {recipientAccountConversation.Account.LastName}";
                conversationModel.Image = recipientAccountConversation.Account.Image;
                conversationModel.IsAllowed =
                    !recipientAccountConversation.IsDeleted && !recipientAccountConversation.Account.IsDeleted;
            }
            else
            {
                if (conversationModel.Name == null)
                {
                    string conversationName = "";
                    foreach (var accountConversation in conversation.AccountConversations
                                 .Where(accountConversation => accountConversation.AccountId != currentUserId &&
                                                               !accountConversation.IsDeleted &&
                                                               !accountConversation.Account.IsDeleted).Take(5))
                    {
                        // Add a comma and space only if this is not the first name
                        if (!string.IsNullOrEmpty(conversationName))
                        {
                            conversationName += ", ";
                        }

                        conversationName += $"{accountConversation.Account.FirstName}";
                    }

                    conversationModel.Name = conversationName;
                }

                conversationModel.IsAllowed = !existedAccountConversation.IsDeleted;
            }

            return new ResponseModel
            {
                Message = "Get conversation successfully",
                Data = conversationModel
            };
        });

        return responseModel;
    }
}