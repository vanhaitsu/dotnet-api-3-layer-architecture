using Services.Models.MessageModels;
using Services.Models.ResponseModels;

namespace Services.Interfaces;

public interface IMessageService
{
    Task<ResponseModel> Add(MessageAddModel messageAddModel);
}