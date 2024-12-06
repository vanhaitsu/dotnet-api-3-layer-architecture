using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Configuration;
using Repositories.Common;
using Repositories.Entities;
using Repositories.Interfaces;
using Repositories.Models.AccountModels;
using Services.Common;
using Services.Interfaces;
using Services.Models.AccountModels;
using Services.Models.ResponseModels;
using Services.Models.TokenModels;
using Services.Utils;
using Role = Repositories.Enums.Role;

namespace Services.Services;

public class AccountService : IAccountService
{
    private readonly IClaimService _claimService;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly IRedisHelper _redisHelper;
    private readonly IUnitOfWork _unitOfWork;

    public AccountService(IClaimService claimService, IConfiguration configuration, IEmailService emailService,
        IMapper mapper, IRedisHelper redisHelper, IUnitOfWork unitOfWork)
    {
        _claimService = claimService;
        _configuration = configuration;
        _emailService = emailService;
        _mapper = mapper;
        _redisHelper = redisHelper;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseModel> SignUp(AccountSignUpModel accountSignUpModel)
    {
        var existedEmail = await _unitOfWork.AccountRepository.FindByEmailAsync(accountSignUpModel.Email);
        if (existedEmail != null)
            return new ResponseModel
            {
                Code = StatusCodes.Status409Conflict,
                Message = "Email already exists"
            };

        if (!string.IsNullOrWhiteSpace(accountSignUpModel.Username))
        {
            var existedUsername = await _unitOfWork.AccountRepository.FindByUsernameAsync(accountSignUpModel.Username);
            if (existedUsername != null)
                return new ResponseModel
                {
                    Code = StatusCodes.Status409Conflict,
                    Message = "Username already exists"
                };
        }
        else
        {
            accountSignUpModel.Username = AuthenticationTools.GenerateUniqueToken(DateTime.UtcNow)
                .Replace("/", string.Empty).Replace("+", string.Empty).Replace("-", string.Empty);
        }

        var account = _mapper.Map<Account>(accountSignUpModel);
        account.HashedPassword = AuthenticationTools.HashPassword(accountSignUpModel.Password);
        account.VerificationCode = AuthenticationTools.GenerateDigitCode(Constant.VerificationCodeLength);
        account.VerificationCodeExpiryTime = DateTime.UtcNow.AddMinutes(Constant.VerificationCodeValidityInMinutes);
        await _unitOfWork.AccountRepository.AddAsync(account);

        // Add "user" role as default
        var role = await _unitOfWork.RoleRepository.FindByNameAsync(Role.User.ToString());
        var accountRole = new AccountRole
        {
            Account = account,
            Role = role!
        };
        await _unitOfWork.AccountRoleRepository.AddAsync(accountRole);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            // Email verification
            await SendVerificationEmail(account);
            await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

            return new ResponseModel
            {
                Code = StatusCodes.Status201Created,
                Message = "Sign up successfully, please verify your email"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot sign up"
        };
    }

    public async Task<ResponseModel> SignIn(AccountSignInModel accountSignInModel)
    {
        var account = await _unitOfWork.AccountRepository.FindByEmailAsync(accountSignInModel.Email);
        if (account != null)
        {
            if (account.IsDeleted)
                return new ResponseModel
                {
                    Code = StatusCodes.Status410Gone,
                    Message = "Account has been deleted"
                };

            if (AuthenticationTools.VerifyPassword(accountSignInModel.Password, account.HashedPassword))
            {
                var tokenModel = await GenerateJwtToken(account);
                if (tokenModel != null)
                    return new ResponseModel
                    {
                        Message = "Sign in successfully",
                        Data = tokenModel
                    };

                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Cannot sign in"
                };
            }
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status401Unauthorized,
            Message = "Invalid email or password"
        };
    }

    public async Task<ResponseModel> RefreshToken(AccountRefreshTokenModel accountRefreshTokenModel)
    {
        if (!accountRefreshTokenModel.DeviceId.HasValue ||
            string.IsNullOrWhiteSpace(accountRefreshTokenModel.AccessToken) ||
            string.IsNullOrWhiteSpace(accountRefreshTokenModel.RefreshToken))
            return new ResponseModel
            {
                Code = StatusCodes.Status400BadRequest,
                Message = "Invalid information"
            };

        var refreshToken =
            await _unitOfWork.RefreshTokenRepository.FindByDeviceIdAsync(accountRefreshTokenModel.DeviceId.Value);
        if (refreshToken == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Device not found"
            };

        // Validate access token
        var principal =
            AuthenticationTools.GetPrincipalFromExpiredToken(accountRefreshTokenModel.AccessToken, _configuration);
        if (principal == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Invalid access token"
            };

        var accountIdFromPrincipal = principal.FindFirst("accountId")?.Value;
        if (string.IsNullOrWhiteSpace(accountIdFromPrincipal) ||
            !Guid.TryParse(accountIdFromPrincipal, out var accountId))
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Invalid account"
            };

        // Validate refresh token
        var account = await _unitOfWork.AccountRepository.GetAsync(accountId);
        if (account == null || account.IsDeleted || refreshToken.AccountId != account.Id ||
            refreshToken.Token != accountRefreshTokenModel.RefreshToken)
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Invalid refresh token"
            };

        var tokenModel = await GenerateJwtToken(account, refreshToken, principal);
        if (tokenModel != null)
            return new ResponseModel
            {
                Message = "Refresh token successfully",
                Data = tokenModel
            };

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot refresh token"
        };
    }

    public async Task<ResponseModel> RevokeTokens(AccountEmailModel accountEmailModel)
    {
        var refreshTokens =
            await _unitOfWork.RefreshTokenRepository.GetAllAsync(
                refreshToken => refreshToken.Account.Email == accountEmailModel.Email);
        _unitOfWork.RefreshTokenRepository.HardRemoveRange(refreshTokens.Data);
        await _unitOfWork.SaveChangeAsync();

        return new ResponseModel
        {
            Message = "Revoke tokens successfully"
        };
    }

    public async Task<ResponseModel> VerifyEmail(string email, string verificationCode)
    {
        var account = await _unitOfWork.AccountRepository.FindByEmailAsync(email);
        if (account == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account not found"
            };

        if (account.EmailConfirmed)
            return new ResponseModel
            {
                Message = "Email has been verified"
            };

        if (account.VerificationCodeExpiryTime < DateTime.UtcNow)
            return new ResponseModel
            {
                Code = StatusCodes.Status400BadRequest,
                Message = "The code is expired"
            };

        if (account.VerificationCode == verificationCode)
        {
            account.EmailConfirmed = true;
            account.VerificationCode = null;
            account.VerificationCodeExpiryTime = null;
            _unitOfWork.AccountRepository.Update(account);
            if (await _unitOfWork.SaveChangeAsync() > 0)
            {
                await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Id}");
                await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Username}");
                await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

                return new ResponseModel
                {
                    Message = "Verify email successfully"
                };
            }
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status400BadRequest,
            Message = "Cannot verify email"
        };
    }

    public async Task<ResponseModel> ResendVerificationEmail(AccountEmailModel accountEmailModel)
    {
        var account = await _unitOfWork.AccountRepository.FindByEmailAsync(accountEmailModel.Email);
        if (account == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account not found"
            };

        if (account.EmailConfirmed)
            return new ResponseModel
            {
                Message = "Email has been verified"
            };

        // Update new verification code
        account.VerificationCode = AuthenticationTools.GenerateDigitCode(Constant.VerificationCodeLength);
        account.VerificationCodeExpiryTime = DateTime.UtcNow.AddMinutes(Constant.VerificationCodeValidityInMinutes);
        _unitOfWork.AccountRepository.Update(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await SendVerificationEmail(account);

            return new ResponseModel
            {
                Message = "Resend verification email successfully"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot resend verification email"
        };
    }

    public async Task<ResponseModel> ChangePassword(AccountChangePasswordModel accountChangePasswordModel)
    {
        var currentUserId = _claimService.GetCurrentUserId;
        if (!currentUserId.HasValue)
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized"
            };

        var account = await _unitOfWork.AccountRepository.GetAsync(currentUserId.Value);
        if (AuthenticationTools.VerifyPassword(accountChangePasswordModel.OldPassword, account!.HashedPassword))
        {
            account.HashedPassword = AuthenticationTools.HashPassword(accountChangePasswordModel.NewPassword);
            _unitOfWork.AccountRepository.Update(account);
            if (await _unitOfWork.SaveChangeAsync() > 0)
                return new ResponseModel
                {
                    Message = "Change password successfully"
                };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot change password"
        };
    }

    public async Task<ResponseModel> ForgotPassword(AccountEmailModel accountEmailModel)
    {
        var account = await _unitOfWork.AccountRepository.FindByEmailAsync(accountEmailModel.Email);
        if (account == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account not found"
            };

        var resetPasswordToken =
            AuthenticationTools.GenerateUniqueToken(
                DateTime.UtcNow.AddDays(Constant.ResetPasswordTokenValidityInMinutes));
        account.ResetPasswordToken = resetPasswordToken;
        _unitOfWork.AccountRepository.Update(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await _emailService.SendEmailAsync(account.Email, "Reset your password",
                $"Your token is {resetPasswordToken}. The token will expire in {Constant.ResetPasswordTokenValidityInMinutes} minutes.",
                true);

            return new ResponseModel
            {
                Message = "An email has been sent, please check your inbox"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot send email"
        };
    }

    public async Task<ResponseModel> ResetPassword(AccountResetPasswordModel accountResetPasswordModel)
    {
        var account = await _unitOfWork.AccountRepository.FindByEmailAsync(accountResetPasswordModel.Email);
        if (account == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account not found"
            };

        if (accountResetPasswordModel.Token != account.ResetPasswordToken)
            return new ResponseModel
            {
                Code = StatusCodes.Status400BadRequest,
                Message = "Invalid token"
            };

        account.ResetPasswordToken = null;
        account.HashedPassword = AuthenticationTools.HashPassword(accountResetPasswordModel.Password);
        _unitOfWork.AccountRepository.Update(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
            return new ResponseModel
            {
                Message = "Reset password successfully"
            };

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot reset password"
        };
    }

    public async Task<ResponseModel> AddRange(List<AccountSignUpModel> accountSignUpModels)
    {
        var accounts = new List<Account>();
        foreach (var accountSignUpModel in accountSignUpModels)
        {
            if (!string.IsNullOrWhiteSpace(accountSignUpModel.Username))
            {
                var existedUsername =
                    await _unitOfWork.AccountRepository.FindByUsernameAsync(accountSignUpModel.Username);
                if (existedUsername != null)
                    accountSignUpModel.Username = AuthenticationTools.GenerateUniqueToken(DateTime.UtcNow)
                        .Replace("/", string.Empty).Replace("+", string.Empty).Replace("-", string.Empty);
            }
            else
            {
                accountSignUpModel.Username = AuthenticationTools.GenerateUniqueToken(DateTime.UtcNow)
                    .Replace("/", string.Empty).Replace("+", string.Empty).Replace("-", string.Empty);
            }

            var account = _mapper.Map<Account>(accountSignUpModel);
            account.HashedPassword = AuthenticationTools.HashPassword(accountSignUpModel.Password);
            var role = await _unitOfWork.RoleRepository.FindByNameAsync(accountSignUpModel.Role.ToString() ??
                                                                        Role.User.ToString());
            account.AccountRoles.Add(new AccountRole { AccountId = account.Id, RoleId = role!.Id });
            accounts.Add(account);
        }

        await _unitOfWork.AccountRepository.AddRangeAsync(accounts);
        var result = await _unitOfWork.SaveChangeAsync();
        if (result > 0)
        {
            await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

            return new ResponseModel
            {
                Code = StatusCodes.Status201Created,
                Message = $"Add {result / 2} accounts successfully"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = $"Cannot add {accountSignUpModels.Count} accounts"
        };
    }

    public async Task<ResponseModel> Get(string idOrUsername)
    {
        var cacheKey = $"account_{idOrUsername}";
        var responseModel = await _redisHelper.GetOrSetAsync(cacheKey, async () =>
        {
            Account? account;
            if (Guid.TryParse(idOrUsername, out var id))
                account = await _unitOfWork.AccountRepository.GetAsync(id, "AccountRoles.Role");
            else
                account = await _unitOfWork.AccountRepository.FindByUsernameAsync(idOrUsername, "AccountRoles.Role");

            if (account == null)
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Account not found"
                };

            var accountModel = _mapper.Map<AccountModel>(account);

            return new ResponseModel
            {
                Message = "Get account successfully",
                Data = accountModel
            };
        });

        return responseModel;
    }

    public async Task<ResponseModel> GetAll(AccountFilterModel accountFilterModel)
    {
        var cacheKey = $"accounts_{CacheTools.GenerateCacheKey(accountFilterModel)}";
        var responseModel = await _redisHelper.GetOrSetAsync(cacheKey, async () =>
        {
            var accounts = await _unitOfWork.AccountRepository.GetAllAsync(
                account =>
                    account.IsDeleted == accountFilterModel.IsDeleted &&
                    (!accountFilterModel.Gender.HasValue || account.Gender == accountFilterModel.Gender) &&
                    (!accountFilterModel.Role.HasValue || Enumerable
                        .Select(account.AccountRoles, accountRole => accountRole.Role.Name)
                        .Contains(accountFilterModel.Role.ToString())) &&
                    (string.IsNullOrWhiteSpace(accountFilterModel.Search) ||
                     account.FirstName.ToLower().Contains(accountFilterModel.Search.ToLower()) ||
                     account.LastName.ToLower().Contains(accountFilterModel.Search.ToLower()) ||
                     account.Username.ToLower().Contains(accountFilterModel.Search.ToLower()) ||
                     account.Email.ToLower().Contains(accountFilterModel.Search.ToLower())),
                accounts =>
                {
                    switch (accountFilterModel.Order.ToLower())
                    {
                        case "firstName":
                            return accountFilterModel.OrderByDescending
                                ? accounts.OrderByDescending(account => account.FirstName)
                                : accounts.OrderBy(account => account.FirstName);
                        case "lastName":
                            return accountFilterModel.OrderByDescending
                                ? accounts.OrderByDescending(account => account.LastName)
                                : accounts.OrderBy(account => account.LastName);
                        case "dateOfBirth":
                            return accountFilterModel.OrderByDescending
                                ? accounts.OrderByDescending(account => account.DateOfBirth)
                                : accounts.OrderBy(account => account.DateOfBirth);
                        default:
                            return accountFilterModel.OrderByDescending
                                ? accounts.OrderByDescending(account => account.CreationDate)
                                : accounts.OrderBy(account => account.CreationDate);
                    }
                },
                "AccountRoles.Role",
                accountFilterModel.PageIndex,
                accountFilterModel.PageSize
            );
            var accountModels = _mapper.Map<List<AccountModel>>(accounts.Data);
            var result = new Pagination<AccountModel>(accountModels, accountFilterModel.PageIndex,
                accountFilterModel.PageSize, accounts.TotalCount);

            return new ResponseModel
            {
                Message = "Get all accounts successfully",
                Data = result
            };
        });

        return responseModel;
    }

    public async Task<ResponseModel> UpdatePut(Guid id, AccountUpdateModel accountUpdateModel)
    {
        var account = await _unitOfWork.AccountRepository.GetAsync(id);
        if (account == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account not found"
            };

        if (accountUpdateModel.Username != account.Username)
        {
            var existedUsername = await _unitOfWork.AccountRepository.FindByUsernameAsync(accountUpdateModel.Username);
            if (existedUsername != null)
                return new ResponseModel
                {
                    Code = StatusCodes.Status409Conflict,
                    Message = "Username already exists"
                };
        }

        _mapper.Map(accountUpdateModel, account);
        _unitOfWork.AccountRepository.Update(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Id}");
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Username}");
            await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

            return new ResponseModel
            {
                Message = "Update account successfully"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot update account"
        };
    }

    public async Task<ResponseModel> UpdatePatch(Guid id, JsonPatchDocument<Account> patchDoc)
    {
        var account = await _unitOfWork.AccountRepository.GetAsync(id);
        if (account == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account not found"
            };

        patchDoc.ApplyTo(account);
        _unitOfWork.AccountRepository.Update(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Id}");
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Username}");
            await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

            return new ResponseModel
            {
                Message = "Update account successfully"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot update account"
        };
    }

    public async Task<ResponseModel> Delete(Guid id)
    {
        var account = await _unitOfWork.AccountRepository.GetAsync(id);
        if (account == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account not found"
            };

        _unitOfWork.AccountRepository.SoftRemove(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Id}");
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Username}");
            await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

            return new ResponseModel
            {
                Message = "Delete account successfully"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot delete account"
        };
    }

    public async Task<ResponseModel> Restore(Guid id)
    {
        var account = await _unitOfWork.AccountRepository.GetAsync(id);
        if (account == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account not found"
            };

        _unitOfWork.AccountRepository.Restore(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Id}");
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Username}");
            await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

            return new ResponseModel
            {
                Message = "Restore account successfully"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot restore account"
        };
    }

    #region Helper

    private async Task SendVerificationEmail(Account account)
    {
        await _emailService.SendEmailAsync(account.Email, "Verify your email",
            $"Your verification code is {account.VerificationCode}. The code will expire in {Constant.VerificationCodeValidityInMinutes} minutes.",
            true);
    }

    private async Task<TokenModel?> GenerateJwtToken(Account account, RefreshToken? refreshToken = null,
        ClaimsPrincipal? principal = null)
    {
        // Refresh token information
        var authClaims = new List<Claim>();
        var deviceId = Guid.NewGuid();
        var refreshTokenString =
            AuthenticationTools.GenerateUniqueToken(DateTime.UtcNow.AddDays(Constant.RefreshTokenValidityInDays));

        // If refresh token then reuse the claims
        if (refreshToken != null && principal != null)
        {
            authClaims = principal.Claims.ToList();
            refreshToken.Token = refreshTokenString;
            deviceId = refreshToken.DeviceId;
            _unitOfWork.RefreshTokenRepository.Update(refreshToken);
        }

        // If sign in then add claims
        else
        {
            authClaims.Add(new Claim("accountId", account.Id.ToString()));
            authClaims.Add(new Claim("accountEmail", account.Email));
            authClaims.Add(new Claim("deviceId", deviceId.ToString()));
            authClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            var roles = await _unitOfWork.RoleRepository.GetAllByAccountIdAsync(account.Id);
            foreach (var role in roles) authClaims.Add(new Claim(ClaimTypes.Role, role.Name));
            await _unitOfWork.RefreshTokenRepository.AddAsync(new RefreshToken
            {
                DeviceId = deviceId,
                Token = refreshTokenString,
                Account = account
            });
        }

        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            var jwtToken = AuthenticationTools.CreateJwtToken(authClaims, _configuration);

            return new TokenModel
            {
                DeviceId = deviceId,
                AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                RefreshToken = refreshTokenString
            };
        }

        return null;
    }

    #endregion
}