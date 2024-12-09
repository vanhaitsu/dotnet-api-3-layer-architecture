using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Services.Interfaces;
using Services.Models.MessageModels;
using Services.Models.ResponseModels;

namespace API.Controllers;

[Route("api/v1/messages")]
[ApiController]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessageController(IMessageService messageService, IUnitOfWork unitOfWork)
    {
        _messageService = messageService;
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, [FromBody] MessageDeleteModel messageDeleteModel)
    {
        try
        {
            var result = await _messageService.Delete(id, messageDeleteModel);
            return StatusCode(result.Code, result);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel
            {
                Code = StatusCodes.Status500InternalServerError,
                Message = ex.Message
            });
        }
    }
}