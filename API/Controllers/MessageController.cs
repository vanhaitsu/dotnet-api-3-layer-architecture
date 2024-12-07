using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Entities;
using Repositories.Interfaces;
using Services.Interfaces;
using Services.Models.MessageModels;
using Services.Models.ResponseModels;
using Message = Repositories.Entities.Message;

namespace API.Controllers;

[Route("api/v1/messages")]
[ApiController]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IUnitOfWork _unitOfWork;

    public MessageController(IMessageService messageService, IUnitOfWork unitOfWork)
    {
        _messageService = messageService;
        _unitOfWork = unitOfWork;
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
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel
            {
                Code = StatusCodes.Status500InternalServerError,
                Message = ex.Message
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> TestAdd()
    {
        try
        {
            var a = new Guid("01939a15-8d0e-737c-a297-10b4a985b479"); // Current user
            var aConversation = new Guid("01939c71-64cc-76a1-b5c6-96278f94ed69");
            var b = new Guid("01939c29-082f-791e-b5d5-a3b7a3e17fa3");
            var bConversation = new Guid("01939c71-64e3-796a-a898-40c3136b7392");

            var messagesFromA = new List<Message>();
            var messagesFromB = new List<Message>();
            for (var i = 0; i < 500; i++)
            {
                messagesFromA.Add(new Message
                {
                    Content = $"This is a test message from A: {i}",
                    CreatedById = a,
                    MessageRecipients =
                    [
                        new MessageRecipient
                        {
                            AccountId = a,
                            CreatedById = a,
                            AccountConversationId = aConversation
                        },
                        new MessageRecipient
                        {
                            AccountId = b,
                            CreatedById = a,
                            AccountConversationId = bConversation
                        }
                    ]
                });

                messagesFromB.Add(new Message
                {
                    Content = $"This is a test message from B: {i}",
                    CreatedById = b,
                    MessageRecipients =
                    [
                        new MessageRecipient
                        {
                            AccountId = a,
                            CreatedById = b,
                            AccountConversationId = aConversation
                        },
                        new MessageRecipient
                        {
                            AccountId = b,
                            CreatedById = b,
                            AccountConversationId = bConversation
                        }
                    ]
                });
            }

            await _unitOfWork.MessageRepository.AddRangeAsync(messagesFromA);
            await _unitOfWork.MessageRepository.AddRangeAsync(messagesFromB);

            return Ok(await _unitOfWork.SaveChangeAsync());
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