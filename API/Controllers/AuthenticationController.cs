using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Models.AccountModels;

namespace API.Controllers;

[Route("api/v1/authentication")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AuthenticationController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromBody] AccountSignUpModel accountSignUpModel)
    {
        try
        {
            var result = await _accountService.SignUp(accountSignUpModel);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] AccountSignInModel accountSignInModel)
    {
        try
        {
            var result = await _accountService.SignIn(accountSignInModel);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpPost("token/refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] AccountRefreshTokenModel accountRefreshTokenModel)
    {
        try
        {
            #region Get information from cookie

            // DeviceId
            HttpContext.Request.Cookies.TryGetValue("deviceId", out var deviceIdFromCookie);
            if (deviceIdFromCookie != null) accountRefreshTokenModel.DeviceId = Guid.Parse(deviceIdFromCookie);

            // Access token
            HttpContext.Request.Cookies.TryGetValue("accessToken", out var accessTokenFromCookie);
            if (accessTokenFromCookie != null) accountRefreshTokenModel.AccessToken = accessTokenFromCookie;

            // Refresh token
            HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshTokenFromCookie);
            if (refreshTokenFromCookie != null) accountRefreshTokenModel.RefreshToken = refreshTokenFromCookie;

            #endregion

            var result = await _accountService.RefreshToken(accountRefreshTokenModel);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [Authorize]
    [HttpPost("token/revoke")]
    public async Task<IActionResult> RevokeTokens([FromBody] AccountEmailModel accountEmailModel)
    {
        try
        {
            var result = await _accountService.RevokeTokens(accountEmailModel);
            return StatusCode(result.StatusCode, result);
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
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpGet("email/verify")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string verificationCode)
    {
        try
        {
            var result = await _accountService.VerifyEmail(email, verificationCode);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpPost("email/resend-verification")]
    public async Task<IActionResult> ResendVerificationEmail([FromBody] AccountEmailModel accountEmailModel)
    {
        try
        {
            var result = await _accountService.ResendVerificationEmail(accountEmailModel);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [Authorize]
    [HttpPost("password/change")]
    public async Task<IActionResult> ChangePassword([FromBody] AccountChangePasswordModel accountChangePasswordModel)
    {
        try
        {
            var result = await _accountService.ChangePassword(accountChangePasswordModel);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpPost("password/forgot")]
    public async Task<IActionResult> ForgotPassword([FromBody] AccountEmailModel accountEmailModel)
    {
        try
        {
            var result = await _accountService.ForgotPassword(accountEmailModel);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpPost("password/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] AccountResetPasswordModel accountResetPasswordModel)
    {
        try
        {
            var result = await _accountService.ResetPassword(accountResetPasswordModel);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
}