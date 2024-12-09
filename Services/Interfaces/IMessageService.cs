using Services.Models.MessageModels;
using Services.Models.ResponseModels;

namespace Services.Interfaces;

public interface IMessageService
{
    Task<ResponseModel> Delete(Guid id, MessageDeleteModel messageDeleteModel);
}