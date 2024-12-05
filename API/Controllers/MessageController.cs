using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Models.MessageModels;

namespace API.Controllers
{
    [Route("api/v1/messages")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] MessageAddModel messageAddModel)
        {
            try
            {
                var result = await _messageService.Add(messageAddModel);
                return StatusCode(result.Code, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
