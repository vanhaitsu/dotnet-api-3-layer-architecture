using AutoMapper;
using Microsoft.AspNetCore.Http;
using Repositories.Enums;
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

    public async Task<ResponseModel> Delete(Guid id, MessageDeleteModel messageDeleteModel)
    {
        var currentUserId = _claimService.GetCurrentUserId;
        if (!currentUserId.HasValue)
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized"
            };

        if (messageDeleteModel.MessageDeleteType == MessageDeleteType.Everyone)
        {
            var message = await _unitOfWork.MessageRepository.GetAsync(id);
            if (message == null)
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Message not found"
                };

            _unitOfWork.MessageRepository.SoftRemove(message, true);
        }
        else if (messageDeleteModel.MessageDeleteType == MessageDeleteType.You)
        {
            var messageRecipient =
                await _unitOfWork.MessageRecipientRepository.FindByAccountIdAndMessageIdAsync(currentUserId.Value, id);
            if (messageRecipient == null)
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Message not found"
                };

            _unitOfWork.MessageRecipientRepository.SoftRemove(messageRecipient);
        }

        if (await _unitOfWork.SaveChangeAsync() > 0)
            return new ResponseModel
            {
                Message = "Delete message successfully"
            };

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot delete message"
        };
    }
}