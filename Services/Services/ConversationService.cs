using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Entities;
using Repositories.Interfaces;
using Repositories.Models.AccountModels;
using Repositories.Models.ConversationModels;
using Repositories.Models.MessageModels;
using Services.Common;
using Services.Interfaces;
using Services.Models.ConversationModels;
using Services.Models.MessageModels;
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
                await _unitOfWork.ConversationRepository.FindByAccountIdAndConversationIdAsync(currentUserId.Value, id,
                    conversations => EntityFrameworkQueryableExtensions.ThenInclude(
                            conversations.Include(conversation => conversation.AccountConversations),
                            accountConversation => accountConversation.Account)
                        .Include(conversation => conversation.AccountConversations).ThenInclude(accountConversation =>
                            accountConversation.MessageRecipients.Where(messageRecipient => !messageRecipient.IsDeleted)
                                .OrderByDescending(messageRecipient => messageRecipient.Message.CreationDate).Take(6))
                        .ThenInclude(messageRecipient => messageRecipient.Message));
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
                NumberOfUnreadMessages = senderAccountConversations.MessageRecipients.Count(messageRecipient =>
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
        var currentUserId = _claimService.GetCurrentUserId;
        if (!currentUserId.HasValue)
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized"
            };

        var cacheKey = $"conversations_account_{currentUserId}_{CacheTools.GenerateCacheKey(conversationFilterModel)}";
        var responseModel = await _redisHelper.GetOrSetAsync(cacheKey, async () =>
        {
            var conversations = await _unitOfWork.ConversationRepository.GetAllAsync(
                conversation =>
                    Enumerable.Any(conversation.AccountConversations, accountConversation =>
                        accountConversation.AccountId == currentUserId &&
                        (conversationFilterModel.IsArchived == null ||
                         accountConversation.IsArchived == conversationFilterModel.IsArchived) &&
                        !accountConversation.IsDeleted &&
                        accountConversation.MessageRecipients.Any(messageRecipient => !messageRecipient.IsDeleted)) &&
                    (string.IsNullOrWhiteSpace(conversationFilterModel.Search) ||
                     Enumerable.Any(conversation.AccountConversations, accountConversation =>
                         accountConversation.Account.FirstName.ToLower()
                             .Contains(conversationFilterModel.Search.ToLower())) ||
                     Enumerable.Any(conversation.AccountConversations, accountConversation =>
                         accountConversation.Account.LastName.ToLower()
                             .Contains(conversationFilterModel.Search.ToLower())) ||
                     Enumerable.Any(conversation.AccountConversations, accountConversation =>
                         accountConversation.Account.Username.ToLower()
                             .Contains(conversationFilterModel.Search.ToLower())) ||
                     Enumerable.Any(conversation.AccountConversations, accountConversation =>
                         accountConversation.Account.Email.ToLower()
                             .Contains(conversationFilterModel.Search.ToLower()))),
                conversations => conversations.OrderByDescending(conversation =>
                    conversation.AccountConversations
                        .First(accountConversation => accountConversation.AccountId == currentUserId).MessageRecipients
                        .Max(messageRecipient => messageRecipient.Message.CreationDate)),
                conversations => EntityFrameworkQueryableExtensions.ThenInclude(
                        conversations.Include(conversation => conversation.AccountConversations),
                        accountConversation => accountConversation.Account)
                    .Include(conversation => conversation.AccountConversations).ThenInclude(accountConversation =>
                        accountConversation.MessageRecipients.Where(messageRecipient => !messageRecipient.IsDeleted)
                            .OrderByDescending(messageRecipient => messageRecipient.Message.CreationDate).Take(6))
                    .ThenInclude(messageRecipient => messageRecipient.Message),
                conversationFilterModel.PageIndex,
                conversationFilterModel.PageSize
            );
            var conversationModels = new List<ConversationModel>();
            foreach (var conversation in conversations.Data)
            {
                var senderAccountConversations =
                    conversation.AccountConversations.First(accountConversation =>
                        accountConversation.AccountId == currentUserId);
                var recipientAccountConversations =
                    conversation.AccountConversations.First(accountConversation =>
                        accountConversation.AccountId != currentUserId);
                var latestMessage = senderAccountConversations.MessageRecipients
                    .Select(messageRecipient => messageRecipient.Message).FirstOrDefault();
                conversationModels.Add(new ConversationModel
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
                });
            }

            var result = new Pagination<ConversationModel>(conversationModels, conversationFilterModel.PageIndex,
                conversationFilterModel.PageSize, conversations.TotalCount);

            return new ResponseModel
            {
                Message = "Get all conversations successfully",
                Data = result
            };
        });

        return responseModel;
    }

    public async Task<ResponseModel> Archive(Guid id)
    {
        var currentUserId = _claimService.GetCurrentUserId;
        if (!currentUserId.HasValue)
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized"
            };

        var accountConversation =
            await _unitOfWork.AccountConversationRepository.FindByAccountIdAndConversationIdAsync(currentUserId.Value,
                id);
        if (accountConversation == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Conversation not found"
            };

        accountConversation.IsArchived = true;
        _unitOfWork.AccountConversationRepository.Update(accountConversation);
        if (await _unitOfWork.SaveChangeAsync() > 0)
            return new ResponseModel
            {
                Message = "Archive conversation successfully"
            };

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot archive conversation"
        };
    }

    public async Task<ResponseModel> Delete(Guid id)
    {
        var currentUserId = _claimService.GetCurrentUserId;
        if (!currentUserId.HasValue)
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized"
            };

        var accountConversation =
            await _unitOfWork.AccountConversationRepository.FindByAccountIdAndConversationIdAsync(currentUserId.Value,
                id);
        if (accountConversation == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Conversation not found"
            };

        await _unitOfWork.MessageRecipientRepository.SoftRemoveAllByAccountIdAndAccountConversationIdAsync(
            currentUserId.Value, accountConversation.Id);
        if (await _unitOfWork.SaveChangeAsync() > 0)
            return new ResponseModel
            {
                Message = "Delete conversation successfully"
            };

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot delete conversation"
        };
    }

    public async Task<ResponseModel> AddMessage(Guid conversationId, MessageAddModel messageAddModel)
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
                conversationId,
                conversations => conversations.Include(conversation => conversation.AccountConversations));
        if (existedConversation == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Conversation not found"
            };

        var message = _mapper.Map<Message>(messageAddModel);
        var accountConversations = new List<AccountConversation>();
        foreach (var accountConversation in existedConversation.AccountConversations)
        {
            if (accountConversation.IsArchived)
            {
                accountConversation.IsArchived = false;
                accountConversations.Add(accountConversation);
            }

            message.MessageRecipients.Add(new MessageRecipient
            {
                IsRead = accountConversation.AccountId == currentUserId,
                AccountId = accountConversation.AccountId,
                AccountConversationId = accountConversation.Id
            });
        }

        _unitOfWork.AccountConversationRepository.UpdateRange(accountConversations);
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

    public async Task<ResponseModel> GetAllMessages(Guid conversationId, MessageFilterModel messageFilterModel)
    {
        var currentUserId = _claimService.GetCurrentUserId;
        if (!currentUserId.HasValue)
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized"
            };

        var cacheKey =
            $"messages_conversation_{conversationId}_account_{currentUserId}_{CacheTools.GenerateCacheKey(messageFilterModel)}";
        var responseModel = await _redisHelper.GetOrSetAsync(cacheKey, async () =>
        {
            var messages = await _unitOfWork.MessageRepository.GetAllAsync(
                message => Enumerable.Any(message.MessageRecipients, messageRecipient =>
                    messageRecipient.AccountId == currentUserId &&
                    messageRecipient.AccountConversation.ConversationId == conversationId &&
                    !messageRecipient.IsDeleted),
                messages => messages.OrderByDescending(message => message.CreationDate),
                messages => EntityFrameworkQueryableExtensions.Include(messages.Include(message => message.CreatedBy),
                        message => message.MessageRecipients)
                    .ThenInclude(messageRecipient => messageRecipient.Account),
                messageFilterModel.PageIndex,
                messageFilterModel.PageSize
            );
            var messageModels = new List<MessageModel>();
            var unreadMessages = new List<MessageRecipient>();
            foreach (var message in messages.Data)
            {
                var currentUserMessageRecipient =
                    message.MessageRecipients.First(messageRecipient => messageRecipient.AccountId == currentUserId);
                if (!currentUserMessageRecipient.IsRead)
                {
                    currentUserMessageRecipient.IsRead = true;
                    unreadMessages.Add(currentUserMessageRecipient);
                }

                messageModels.Add(new MessageModel
                {
                    Id = message.Id,
                    CreationDate = message.CreationDate,
                    CreatedById = message.CreatedById,
                    IsDeleted = message.IsDeleted,
                    Content = message.IsDeleted ? null : message.Content,
                    AttachmentUrl = message.IsDeleted ? null : message.AttachmentUrl,
                    MessageType = message.MessageType,
                    IsPinned = message.IsPinned,
                    IsModified = message.ModificationDate != null || message.ModifiedById != null,
                    ParentMessageId = message.ParentMessageId,
                    IsReadBy = _mapper.Map<List<AccountLiteModel>>(
                        message.MessageRecipients
                            .Where(messageRecipient => messageRecipient.AccountId != currentUserId &&
                                                       messageRecipient.AccountId != message.CreatedById &&
                                                       messageRecipient.IsRead)
                            .Select(messageRecipient => messageRecipient.Account))
                });
            }

            if (unreadMessages.Any())
            {
                _unitOfWork.MessageRecipientRepository.UpdateRange(unreadMessages);
                if (await _unitOfWork.SaveChangeAsync() != unreadMessages.Count)
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = "Cannot get all messages"
                    };
            }

            var result = new Pagination<MessageModel>(messageModels, messageFilterModel.PageIndex,
                messageFilterModel.PageSize, messages.TotalCount);

            return new ResponseModel
            {
                Message = "Get all messages successfully",
                Data = result
            };
        });

        return responseModel;
    }
}