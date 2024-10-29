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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AccountRegisterModel accountRegisterModel)
    {
        try
        {
            var result = await _accountService.Register(accountRegisterModel);
            if (result.Status) return Ok(result);

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AccountLoginModel accountLoginModel)
    {
        try
        {
            var result = await _accountService.Login(accountLoginModel);
            if (result.Status) return Ok(result);

            return BadRequest(result);
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
            if (result.Status) return Ok(result);

            return BadRequest(result);
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
            if (result.Status) return Ok(result);

            return BadRequest(result);
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
            if (result.Status) return Ok(result);

            return BadRequest(result);
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
            if (result.Status) return Ok(result);

            return BadRequest(result);
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
            if (result.Status) return Ok(result);

            return BadRequest(result);
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
            if (result.Status) return Ok(result);

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
}