using Services.Models.ConversationModels;
using Services.Models.ResponseModels;

namespace Services.Interfaces;

public interface IConversationService
{
    Task<ResponseModel> Add(ConversationAddModel conversationAddModel);
}