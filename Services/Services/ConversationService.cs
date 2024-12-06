using AutoMapper;
using Microsoft.AspNetCore.Http;
using Repositories.Entities;
using Repositories.Interfaces;
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

        var recipient = await _unitOfWork.AccountRepository.GetAsync(conversationAddModel.RecipientId);
        if (recipient == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Recipient not found"
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
                    Account = recipient
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
        // var currentUserId = _claimService.GetCurrentUserId;
        // if (!currentUserId.HasValue)
        //     return new ResponseModel
        //     {
        //         Code = StatusCodes.Status401Unauthorized,
        //         Message = "Unauthorized"
        //     };
        //
        // var cacheKey = $"conversation_{id}_account_{currentUserId}";
        // var responseModel = await _redisHelper.GetOrSetAsync(cacheKey, async () =>
        // {
        //     var accountConversation =
        //         await _unitOfWork.AccountConversationRepository.FindAndMapByAccountIdAndConversationIdAsync(
        //             currentUserId.Value, id);
        //     if (accountConversation != null)
        //         return new ResponseModel
        //         {
        //             Code = StatusCodes.Status404NotFound,
        //             Message = "Conversation not found"
        //         };
        //
        //     
        //
        //     return new ResponseModel();
        // });
        //
        // return responseModel;

        return new ResponseModel();
    }

    public async Task<ResponseModel> GetAll(ConversationFilterModel conversationFilterModel)
    {
        return new ResponseModel();
    }
}