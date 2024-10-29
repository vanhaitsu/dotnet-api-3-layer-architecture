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

    [HttpPost("range")]
    public async Task<IActionResult> AddRange([FromBody] List<AccountRegisterModel> accountRegisterModels)
    {
        try
        {
            var result = await _accountService.AddRange(accountRegisterModels);
            if (result.Status) return Ok(result);

            return BadRequest(result);
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
            if (result.Status) return Ok(result);

            return BadRequest(result);
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
            if (result.Status) return Ok(result);

            return BadRequest(result);
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
            if (result.Status) return Ok(result);

            return BadRequest(result);
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
            if (result.Status) return Ok(result);

            return BadRequest(result);
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
            if (result.Status) return Ok(result);

            return BadRequest(result);
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
            if (result.Status) return Ok(result);

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
}