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
    private readonly IUnitOfWork _unitOfWork;

    public ConversationService(IClaimService claimService, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _claimService = claimService;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseModel> Add(ConversationAddModel conversationAddModel)
    {
        var currentUserId = _claimService.GetCurrentUserId;
        if (currentUserId == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status403Forbidden,
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
}