using AutoMapper;
using Microsoft.AspNetCore.Http;
using Repositories.Common;
using Repositories.Entities;
using Repositories.Interfaces;
using Repositories.Models.AccountModels;
using Repositories.Models.ConversationModels;
using Repositories.Models.MessageModels;
using Services.Common;
using Services.Interfaces;
using Services.Models.ConversationModels;
using Services.Models.ResponseModels;
using Services.Utils;

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

        var accountIdWithCurrentUserId = new List<Guid>(conversationAddModel.AccountIds.Distinct())
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

        if (validAccountIds.Count > Constant.MaxNumberOfMembersInConversation)
            return new ResponseModel
            {
                Code = StatusCodes.Status400BadRequest,
                Message = "Too many accounts"
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
            accountConversations.Add(new AccountConversation
            {
                IsOwner = accountId == currentUserId,
                AccountId = accountId,
                Conversation = conversation
            });

        await _unitOfWork.AccountConversationRepository.AddRangeAsync(accountConversations);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            return new ResponseModel
            {
                Code = StatusCodes.Status201Created,
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

        var cacheKey = $"conversation_{id}_account_{currentUserId}";
        var responseModel = await _redisHelper.GetOrSetAsync(cacheKey, async () =>
        {
            var accountConversation =
                await _unitOfWork.AccountConversationRepository.FindByAccountIdAndConversationIdAsync(
                    currentUserId.Value, id);
            if (accountConversation == null)
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Conversation not found"
                };

            var members = await _unitOfWork.AccountConversationRepository.GetAllActiveMembersByConversationIdAsync(id);
            var conversationModel = _mapper.Map<ConversationModel>(accountConversation.Conversation);
            conversationModel.IsActive = members.Count >= 2;
            conversationModel.IsGroup = members.Count > 2;
            conversationModel.NumberOfMembers = members.Count;
            conversationModel.Members =
                _mapper.Map<List<MemberModel>>(members.Where(account => account.Id != currentUserId)
                    .OrderBy(account => account.Username).Take(3));
            if (conversationModel.IsActive)
            {
                if (conversationModel.IsGroup && accountConversation.Conversation.Name == null)
                {
                    var conversationName = string.Empty;
                    foreach (var account in members.Where(account => account.Id != currentUserId)
                                 .OrderBy(account => account.Username).Take(5))
                    {
                        // Add a comma and space only if this is not the first name
                        if (!string.IsNullOrWhiteSpace(conversationName)) conversationName += ", ";

                        conversationName += $"{account.FirstName}";
                    }

                    conversationModel.Name = conversationName;
                }
                else
                {
                    var recipientAccountConversation = members.FirstOrDefault(account => account.Id != currentUserId)!;
                    conversationModel.Name =
                        $"{recipientAccountConversation.FirstName} {recipientAccountConversation.LastName}";
                    conversationModel.Image = recipientAccountConversation.Image;
                }
            }

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
        var currentUserId = _claimService.GetCurrentUserId;
        if (currentUserId == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized"
            };

        var cacheKey = $"conversations_account_{currentUserId}_{CacheTools.GenerateCacheKey(conversationFilterModel)}";
        var responseModel = await _redisHelper.GetOrSetAsync(cacheKey, async () =>
        {
            var accountConversations = await _unitOfWork.AccountConversationRepository.GetAllAsync(
                accountConversation => !accountConversation.IsDeleted &&
                                       accountConversation.AccountId == currentUserId &&
                                       !accountConversation.Conversation.IsDeleted,
                accountConversations => accountConversations.OrderBy(accountConversation =>
                    accountConversation.MessageRecipients
                        .OrderByDescending(messageRecipient => messageRecipient.CreationDate)
                        .FirstOrDefault(messageRecipient => messageRecipient.AccountId == currentUserId)),
                "MessageRecipients.Message, Conversation.AccountConversations.Account",
                conversationFilterModel.PageIndex,
                conversationFilterModel.PageSize
            );
            var conversationModels = accountConversations.Data.Select(accountConversation =>
            {
                var members =
                    accountConversation.Conversation.AccountConversations.Select(conversation => conversation.Account)
                        .ToList();
                var conversationModel = _mapper.Map<ConversationModel>(accountConversation.Conversation);
                conversationModel.IsActive = members.Count >= 2;
                conversationModel.IsGroup = members.Count > 2;
                conversationModel.NumberOfMembers = members.Count;
                conversationModel.Members =
                    _mapper.Map<List<MemberModel>>(members.Where(account => account.Id != currentUserId)
                        .OrderBy(account => account.Username).Take(3));
                if (conversationModel.IsActive)
                {
                    if (conversationModel.IsGroup && accountConversation.Conversation.Name == null)
                    {
                        var conversationName = string.Empty;
                        foreach (var account in members.Where(account => account.Id != currentUserId)
                                     .OrderBy(account => account.Username).Take(5))
                        {
                            // Add a comma and space only if this is not the first name
                            if (!string.IsNullOrWhiteSpace(conversationName)) conversationName += ", ";

                            conversationName += $"{account.FirstName}";
                        }

                        conversationModel.Name = conversationName;
                    }
                    else
                    {
                        var recipientAccountConversation =
                            members.FirstOrDefault(account => account.Id != currentUserId)!;
                        conversationModel.Name =
                            $"{recipientAccountConversation.FirstName} {recipientAccountConversation.LastName}";
                        conversationModel.Image = recipientAccountConversation.Image;
                    }
                }

                conversationModel.NumberOfUnreadMessages = accountConversation.MessageRecipients
                    .Where(messageRecipient => messageRecipient.AccountId == currentUserId).Count();
                var latestMessage = accountConversation.MessageRecipients
                    .OrderByDescending(messageRecipient => messageRecipient.CreationDate)
                    .FirstOrDefault();
                if (latestMessage != null)
                {
                    conversationModel.LatestMessage = new LatestMessageModel
                    {
                        Message = latestMessage.Message.Body,
                        AccountId = latestMessage.AccountId,
                        SenderFirstName = latestMessage.Account.FirstName,
                    };
                }

                return conversationModel;
            });
            var result = new Pagination<ConversationModel>(conversationModels.ToList(),
                conversationFilterModel.PageIndex,
                conversationFilterModel.PageSize, accountConversations.TotalCount);

            return new ResponseModel
            {
                Message = "Get all conversations successfully",
                Data = result
            };
        });

        return responseModel;
    }

    public async Task<ResponseModel> GetAllMembers(Guid id)
    {
        var currentUserId = _claimService.GetCurrentUserId;
        if (currentUserId == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized"
            };

        var cacheKey = $"conversation_members_{id}";
        var responseModel = await _redisHelper.GetOrSetAsync(cacheKey, async () =>
        {
            var members = await _unitOfWork.AccountConversationRepository.GetAllActiveMembersByConversationIdAsync(id);
            var membersModel = _mapper.Map<List<MemberModel>>(members);

            return new ResponseModel
            {
                Message = "Get all members successfully",
                Data = membersModel
            };
        });

        return responseModel;
    }
}