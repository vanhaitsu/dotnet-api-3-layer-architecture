using Services.Models.ConversationModels;
using Services.Models.ResponseModels;

namespace Services.Interfaces;

public interface IConversationService
{
    Task<ResponseModel> Add(ConversationAddModel conversationAddModel);
    Task<ResponseModel> Get(Guid id);
    Task<ResponseModel> GetAll(ConversationFilterModel conversationFilterModel);
    Task<ResponseModel> Archive(Guid id);
    Task<ResponseModel> Delete(Guid id);
}