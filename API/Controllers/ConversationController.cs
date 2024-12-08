using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Models.ConversationModels;
using Services.Models.ResponseModels;

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
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel
            {
                Code = StatusCodes.Status500InternalServerError,
                Message = ex.Message
            });
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
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel
            {
                Code = StatusCodes.Status500InternalServerError,
                Message = ex.Message
            });
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ConversationFilterModel conversationFilterModel)
    {
        try
        {
            var result = await _conversationService.GetAll(conversationFilterModel);
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

    [HttpPut("{id}/archive")]
    public async Task<IActionResult> UpdatePut(Guid id)
    {
        try
        {
            var result = await _conversationService.Archive(id);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _conversationService.Delete(id);
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