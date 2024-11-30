using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Models.ConversationModels;

namespace API.Controllers;

[Route("api/v1/conversations")]
[ApiController]
public class ConversationController : ControllerBase
{
    private readonly IConversationService _conversationService;

    public ConversationController(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] ConversationAddModel conversationAddModel)
    {
        try
        {
            var result = await _conversationService.Add(conversationAddModel);
            return StatusCode(result.Code, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
    
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            var result = await _conversationService.Get(id);
            return StatusCode(result.Code, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
}