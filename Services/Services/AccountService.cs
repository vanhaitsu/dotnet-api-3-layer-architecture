using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Configuration;
using Repositories.Common;
using Repositories.Entities;
using Repositories.Interfaces;
using Repositories.Models.AccountModels;
using Services.Common;
using Services.Interfaces;
using Services.Models.AccountModels;
using Services.Models.ResponseModel;
using Services.Models.TokenModels;
using Services.Utils;
using Role = Repositories.Enums.Role;

namespace Services.Services;

public class AccountService : IAccountService
{
    private readonly IRedisHelper _redisHelper;
    private readonly IClaimService _claimService;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public AccountService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration,
        IEmailService emailService, IClaimService claimService, IRedisHelper redisHelper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;
        _emailService = emailService;
        _claimService = claimService;
        _redisHelper = redisHelper;
    }

    public async Task<ResponseModel> Register(AccountRegisterModel accountRegisterModel)
    {
        var existedAccount = await _unitOfWork.AccountRepository.FindByEmailAsync(accountRegisterModel.Email);
        if (existedAccount != null)
            return new ResponseModel
            {
                Status = false,
                Message = "Email already exists"
            };

        var account = _mapper.Map<Account>(accountRegisterModel);
        account.Username = accountRegisterModel.Email.ToLower();
        account.HashedPassword = AuthenticationTools.HashPassword(accountRegisterModel.Password);
        account.VerificationCode = AuthenticationTools.GenerateDigitCode(6);
        account.VerificationCodeExpiryTime = DateTime.Now.AddMinutes(15);
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
            // Email verification
            // await SendVerificationEmail(account);
            return new ResponseModel
            {
                Status = true,
                Message = "Account has been created successfully, please verify your email"
            };

        return new ResponseModel
        {
            Status = false,
            Message = "Cannot create account"
        };
    }

    public async Task<ResponseModel> Login(AccountLoginModel accountLoginModel)
    {
        var account = await _unitOfWork.AccountRepository.FindByEmailAsync(accountLoginModel.Email);
        if (account != null)
        {
            if (account.IsDeleted)
                return new ResponseModel
                {
                    Status = false,
                    Message = "Account has been deleted"
                };

            if (AuthenticationTools.VerifyPassword(accountLoginModel.Password, account.HashedPassword))
            {
                var tokenModel = await GenerateJwtToken(account);
                if (tokenModel != null)
                    return new ResponseModel
                    {
                        Status = true,
                        Message = "Login successfully",
                        Data = tokenModel
                    };

                return new ResponseModel
                {
                    Status = false,
                    Message = "Cannot login"
                };
            }
        }

        return new ResponseModel
        {
            Status = false,
            Message = "Invalid email or password"
        };
    }

    public async Task<ResponseModel> RefreshToken(AccountRefreshTokenModel accountRefreshTokenModel)
    {
        if (!accountRefreshTokenModel.DeviceId.HasValue || accountRefreshTokenModel.AccessToken == null ||
            accountRefreshTokenModel.RefreshToken == null)
            return new ResponseModel
            {
                Status = false,
                Message = "Invalid information"
            };

        var refreshToken =
            await _unitOfWork.RefreshTokenRepository.FindByDeviceIdAsync(accountRefreshTokenModel.DeviceId.Value);
        if (refreshToken == null)
            return new ResponseModel
            {
                Status = false,
                Message = "Device not found"
            };

        // Validate access token and refresh token
        var principal =
            AuthenticationTools.GetPrincipalFromExpiredToken(accountRefreshTokenModel.AccessToken, _configuration);
        var account = await _unitOfWork.AccountRepository.GetAsync(Guid.Parse(principal!.FindFirst("userId")!.Value));
        if (account == null || account.IsDeleted || refreshToken.AccountId != account.Id ||
            refreshToken.Token != accountRefreshTokenModel.RefreshToken)
            return new ResponseModel
            {
                Status = false,
                Message = "Invalid access token or refresh token"
            };

        var tokenModel = await GenerateJwtToken(account, refreshToken, principal);
        if (tokenModel != null)
            return new ResponseModel
            {
                Status = true,
                Message = "Refresh token successfully",
                Data = tokenModel
            };

        return new ResponseModel
        {
            Status = false,
            Message = "Cannot refresh token"
        };
    }

    public async Task<ResponseModel> RevokeTokens(AccountEmailModel accountEmailModel)
    {
        var refreshTokens =
            await _unitOfWork.RefreshTokenRepository.GetAllAsync(
                x => x.Account.Email == accountEmailModel.Email);
        _unitOfWork.RefreshTokenRepository.HardDeleteRange(refreshTokens.Data);
        await _unitOfWork.SaveChangeAsync();

        return new ResponseModel
        {
            Status = true,
            Message = "Revoke tokens successfully"
        };
    }

    public async Task<ResponseModel> VerifyEmail(string email, string verificationCode)
    {
        var account = await _unitOfWork.AccountRepository.FindByEmailAsync(email);
        if (account == null)
            return new ResponseModel
            {
                Status = false,
                Message = "Account not found"
            };

        if (account.EmailConfirmed)
            return new ResponseModel
            {
                Status = true,
                Message = "Email has been verified"
            };

        if (account.VerificationCodeExpiryTime < DateTime.Now)
            return new ResponseModel
            {
                Status = false,
                Message = "The code is expired"
            };

        if (account.VerificationCode == verificationCode)
        {
            account.EmailConfirmed = true;
            account.VerificationCode = null;
            account.VerificationCodeExpiryTime = null;
            _unitOfWork.AccountRepository.Update(account);
            if (await _unitOfWork.SaveChangeAsync() > 0)
                return new ResponseModel
                {
                    Status = true,
                    Message = "Verify email successfully"
                };
        }

        return new ResponseModel
        {
            Status = false,
            Message = "Cannot verify email"
        };
    }

    public async Task<ResponseModel> ResendVerificationEmail(AccountEmailModel accountEmailModel)
    {
        var account = await _unitOfWork.AccountRepository.FindByEmailAsync(accountEmailModel.Email);
        if (account == null)
            return new ResponseModel
            {
                Status = false,
                Message = "Account not found"
            };

        if (account.EmailConfirmed)
            return new ResponseModel
            {
                Status = true,
                Message = "Email has been verified"
            };

        // Update new verification code
        account.VerificationCode = AuthenticationTools.GenerateDigitCode(6);
        account.VerificationCodeExpiryTime = DateTime.Now.AddMinutes(15);
        _unitOfWork.AccountRepository.Update(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await SendVerificationEmail(account);

            return new ResponseModel
            {
                Status = true,
                Message = "Resend verification email successfully"
            };
        }

        return new ResponseModel
        {
            Status = false,
            Message = "Cannot resend verification email"
        };
    }

    public async Task<ResponseModel> ChangePassword(AccountChangePasswordModel accountChangePasswordModel)
    {
        var currentUserId = _claimService.GetCurrentUserId;
        var account = await _unitOfWork.AccountRepository.GetAsync(currentUserId!.Value);
        if (AuthenticationTools.VerifyPassword(accountChangePasswordModel.OldPassword, account!.HashedPassword))
        {
            account.HashedPassword = AuthenticationTools.HashPassword(accountChangePasswordModel.NewPassword);
            _unitOfWork.AccountRepository.Update(account);
            if (await _unitOfWork.SaveChangeAsync() > 0)
                return new ResponseModel
                {
                    Status = true,
                    Message = "Change password successfully"
                };
        }

        return new ResponseModel
        {
            Status = false,
            Message = "Cannot change password"
        };
    }

    public async Task<ResponseModel> ForgotPassword(AccountEmailModel accountEmailModel)
    {
        var account = await _unitOfWork.AccountRepository.FindByEmailAsync(accountEmailModel.Email);
        if (account == null)
            return new ResponseModel
            {
                Status = false,
                Message = "Account not found"
            };

        var resetPasswordToken = AuthenticationTools.GenerateUniqueToken(DateTime.Now.AddDays(15));
        account.ResetPasswordToken = resetPasswordToken;
        _unitOfWork.AccountRepository.Update(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await _emailService.SendEmailAsync(account.Email, "Reset your password",
                $"Your token is {resetPasswordToken}. The token will expire in 15 minutes.", true);

            return new ResponseModel
            {
                Status = true,
                Message = "An email has been sent, please check your inbox"
            };
        }

        return new ResponseModel
        {
            Status = false,
            Message = "Cannot send email"
        };
    }

    public async Task<ResponseModel> ResetPassword(AccountResetPasswordModel accountResetPasswordModel)
    {
        var account = await _unitOfWork.AccountRepository.FindByEmailAsync(accountResetPasswordModel.Email);
        if (account == null)
            return new ResponseModel
            {
                Status = false,
                Message = "Account not found"
            };

        if (accountResetPasswordModel.Token != account.ResetPasswordToken)
            return new ResponseModel
            {
                Status = false,
                Message = "Invalid token"
            };

        account.ResetPasswordToken = null;
        account.HashedPassword = AuthenticationTools.HashPassword(accountResetPasswordModel.Password);
        _unitOfWork.AccountRepository.Update(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
            return new ResponseModel
            {
                Status = true,
                Message = "Reset password successfully"
            };

        return new ResponseModel
        {
            Status = false,
            Message = "Cannot reset password"
        };
    }

    public async Task<ResponseModel> AddRange(List<AccountRegisterModel> accountRegisterModels)
    {
        var accounts = new List<Account>();
        foreach (var accountRegisterModel in accountRegisterModels)
        {
            var account = _mapper.Map<Account>(accountRegisterModel);
            account.Username = accountRegisterModel.Email.ToLower();
            account.HashedPassword = AuthenticationTools.HashPassword(accountRegisterModel.Password);
            var role = await _unitOfWork.RoleRepository.FindByNameAsync(accountRegisterModel.Role.ToString() ??
                                                                        Role.User.ToString());
            account.AccountRoles.Add(new AccountRole { AccountId = account.Id, RoleId = role!.Id });
            accounts.Add(account);
        }

        await _unitOfWork.AccountRepository.AddRangeAsync(accounts);
        var result = await _unitOfWork.SaveChangeAsync();
        if (result > 0)
            return new ResponseModel
            {
                Status = true,
                Message = $"Add {result / 2} accounts successfully"
            };

        return new ResponseModel
        {
            Status = false,
            Message = $"Cannot add {accountRegisterModels.Count} accounts"
        };
    }

    public async Task<ResponseModel> Get(Guid id)
    {
        var cacheKey = $"account_{id}";
        var responseModel = await _redisHelper.GetOrSetAsync(cacheKey, async () =>
        {
            var account = await _unitOfWork.AccountRepository.GetAsync(id, "AccountRoles.Role");
            if (account == null)
                return new ResponseModel
                {
                    Status = false,
                    Message = "Account not found"
                };

            var accountModel = _mapper.Map<AccountModel>(account);

            return new ResponseModel
            {
                Status = true,
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
                x =>
                    x.IsDeleted == accountFilterModel.IsDeleted &&
                    (accountFilterModel.Gender == null || x.Gender == accountFilterModel.Gender) &&
                    (accountFilterModel.Role == null || x.AccountRoles.Select(x => x.Role.Name)
                        .Contains(accountFilterModel.Role.ToString())) &&
                    (string.IsNullOrEmpty(accountFilterModel.Search) ||
                     x.FirstName.ToLower().Contains(accountFilterModel.Search.ToLower()) ||
                     x.LastName.ToLower().Contains(accountFilterModel.Search.ToLower()) ||
                     x.Email.ToLower().Contains(accountFilterModel.Search.ToLower())),
                x =>
                {
                    switch (accountFilterModel.Order.ToLower())
                    {
                        case "firstName":
                            return accountFilterModel.OrderByDescending
                                ? x.OrderByDescending(x => x.FirstName)
                                : x.OrderBy(x => x.FirstName);
                        case "lastName":
                            return accountFilterModel.OrderByDescending
                                ? x.OrderByDescending(x => x.LastName)
                                : x.OrderBy(x => x.LastName);
                        case "dateOfBirth":
                            return accountFilterModel.OrderByDescending
                                ? x.OrderByDescending(x => x.DateOfBirth)
                                : x.OrderBy(x => x.DateOfBirth);
                        default:
                            return accountFilterModel.OrderByDescending
                                ? x.OrderByDescending(x => x.CreationDate)
                                : x.OrderBy(x => x.CreationDate);
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
                Status = true,
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
                Status = false,
                Message = "Account not found"
            };

        _mapper.Map(accountUpdateModel, account);
        _unitOfWork.AccountRepository.Update(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{id}");
            await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

            return new ResponseModel
            {
                Status = true,
                Message = "Update account successfully"
            };
        }

        return new ResponseModel
        {
            Status = false,
            Message = "Cannot update account"
        };
    }

    public async Task<ResponseModel> UpdatePatch(Guid id, JsonPatchDocument<Account> patchDoc)
    {
        var account = await _unitOfWork.AccountRepository.GetAsync(id);
        if (account == null)
            return new ResponseModel
            {
                Status = false,
                Message = "Account not found"
            };

        patchDoc.ApplyTo(account);
        _unitOfWork.AccountRepository.Update(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{id}");
            await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

            return new ResponseModel
            {
                Status = true,
                Message = "Update account successfully"
            };
        }

        return new ResponseModel
        {
            Status = false,
            Message = "Cannot update account"
        };
    }

    public async Task<ResponseModel> Delete(Guid id)
    {
        var account = await _unitOfWork.AccountRepository.GetAsync(id);
        if (account == null)
            return new ResponseModel
            {
                Status = false,
                Message = "Account not found"
            };

        _unitOfWork.AccountRepository.SoftDelete(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{id}");
            await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

            return new ResponseModel
            {
                Status = true,
                Message = "Delete account successfully"
            };
        }

        return new ResponseModel
        {
            Status = false,
            Message = "Cannot delete account"
        };
    }

    public async Task<ResponseModel> Restore(Guid id)
    {
        var account = await _unitOfWork.AccountRepository.GetAsync(id);
        if (account == null)
            return new ResponseModel
            {
                Status = false,
                Message = "Account not found"
            };

        _unitOfWork.AccountRepository.Restore(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{id}");
            await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

            return new ResponseModel
            {
                Status = true,
                Message = "Restore account successfully"
            };
        }

        return new ResponseModel
        {
            Status = false,
            Message = "Cannot restore account"
        };
    }

    #region Helper

    private async Task SendVerificationEmail(Account account)
    {
        await _emailService.SendEmailAsync(account.Email, "Verify your email",
            $"Your verification code is {account.VerificationCode}. The code will expire in 15 minutes.", true);
    }

    private async Task<TokenModel?> GenerateJwtToken(Account account, RefreshToken? refreshToken = null,
        ClaimsPrincipal? principal = null)
    {
        // Refresh token information
        var authClaims = new List<Claim>();
        var deviceId = Guid.NewGuid();
        var refreshTokenString =
            AuthenticationTools.GenerateUniqueToken(DateTime.Now.AddDays(Constant.RefreshTokenValidityInDays));
        if (refreshToken != null && principal != null)
        {
            // If refresh token then reuse the claims
            authClaims = principal.Claims.ToList();
            refreshToken.Token = refreshTokenString;
            deviceId = refreshToken.DeviceId;
            _unitOfWork.RefreshTokenRepository.Update(refreshToken);
        }
        else
        {
            // If login then add claims
            authClaims.Add(new Claim("userId", account.Id.ToString()));
            authClaims.Add(new Claim("userEmail", account.Email));
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