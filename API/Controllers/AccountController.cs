using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Repositories.Entities;
using Services.Interfaces;
using Services.Models.AccountModels;

namespace API.Controllers;

[Route("api/v1/accounts")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("range")]
    public async Task<IActionResult> AddRange([FromBody] List<AccountSignUpModel> accountSignUpModels)
    {
        try
        {
            var result = await _accountService.AddRange(accountSignUpModels);
            return StatusCode(result.Code, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            var result = await _accountService.Get(id);
            return StatusCode(result.Code, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AccountFilterModel accountFilterModel)
    {
        try
        {
            var result = await _accountService.GetAll(accountFilterModel);
            return StatusCode(result.Code, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePut(Guid id, [FromBody] AccountUpdateModel accountUpdateModel)
    {
        try
        {
            var result = await _accountService.UpdatePut(id, accountUpdateModel);
            return StatusCode(result.Code, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdatePatch(Guid id, [FromBody] JsonPatchDocument<Account> patchDoc)
    {
        try
        {
            var result = await _accountService.UpdatePatch(id, patchDoc);
            return StatusCode(result.Code, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _accountService.Delete(id);
            return StatusCode(result.Code, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpPut("{id}/restore")]
    public async Task<IActionResult> Restore(Guid id)
    {
        try
        {
            var result = await _accountService.Restore(id);
            return StatusCode(result.Code, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
}