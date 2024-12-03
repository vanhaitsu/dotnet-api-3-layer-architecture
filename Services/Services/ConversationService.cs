using AutoMapper;
using Microsoft.AspNetCore.Http;
using Repositories.Common;
using Repositories.Entities;
using Repositories.Interfaces;
using Repositories.Models.AccountModels;
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
        if (currentUserId == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized"
            };

        var cacheKey = $"conversation_{id}";
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
            conversationModel.Members = _mapper.Map<List<MemberModel>>(members);
            if (conversationModel.IsActive)
            {
                if (conversationModel.IsGroup && conversationModel.Name == null)
                {
                    var conversationName = string.Empty;
                    foreach (var account in members
                                 .Where(account =>
                                     account.Id != currentUserId && !account.IsDeleted && !account.IsDeleted).Take(5))
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
        // var currentUserId = _claimService.GetCurrentUserId;
        // if (currentUserId == null)
        //     return new ResponseModel
        //     {
        //         Code = StatusCodes.Status401Unauthorized,
        //         Message = "Unauthorized"
        //     };
        //
        // var cacheKey = $"conversations_{currentUserId}_{CacheTools.GenerateCacheKey(conversationFilterModel)}";
        // var responseModel = await _redisHelper.GetOrSetAsync(cacheKey, async () =>
        // {
        //     var accountConversations = await _unitOfWork.AccountConversationRepository.GetAllAsync(
        //         accountConversation => !accountConversation.IsDeleted &&
        //                                accountConversation.AccountId == currentUserId &&
        //                                !accountConversation.Conversation.IsDeleted,
        //         accountConversations => accountConversations.OrderBy(accountConversation => accountConversation.MessageRecipients),
        //         "AccountRoles.Role",
        //         conversationFilterModel.PageIndex,
        //         conversationFilterModel.PageSize
        //     );
        //     var accountModels = _mapper.Map<List<AccountModel>>(accountConversations.Data);
        //     var result = new Pagination<AccountModel>(accountModels, conversationFilterModel.PageIndex,
        //         conversationFilterModel.PageSize, accountConversations.TotalCount);
        //
        //     return new ResponseModel
        //     {
        //         Message = "Get all accounts successfully",
        //         Data = result
        //     };
        // });
        //
        // return responseModel;

        return new ResponseModel();
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

        var cacheKey = $"conversation/members_{id}";
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