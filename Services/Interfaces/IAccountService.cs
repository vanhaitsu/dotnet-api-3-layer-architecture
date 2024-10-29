using Microsoft.AspNetCore.JsonPatch;
using Repositories.Entities;
using Services.Models.AccountModels;
using Services.Models.ResponseModel;

namespace Services.Interfaces;

public interface IAccountService
{
    Task<ResponseModel> Register(AccountRegisterModel accountRegisterModel);
    Task<ResponseModel> Login(AccountLoginModel accountLoginModel);
    Task<ResponseModel> RefreshToken(AccountRefreshTokenModel accountRefreshTokenModel);
    Task<ResponseModel> VerifyEmail(string email, string verificationCode);
    Task<ResponseModel> ResendVerificationEmail(AccountEmailModel accountEmailModel);
    Task<ResponseModel> ChangePassword(AccountChangePasswordModel accountChangePasswordModel);
    Task<ResponseModel> ForgotPassword(AccountEmailModel accountEmailModel);
    Task<ResponseModel> ResetPassword(AccountResetPasswordModel accountResetPasswordModel);
    Task<ResponseModel> AddRange(List<AccountRegisterModel> accountRegisterModels);
    Task<ResponseModel> Get(Guid id);
    Task<ResponseModel> GetAll(AccountFilterModel accountFilterModel);
    Task<ResponseModel> UpdatePut(Guid id, AccountUpdateModel accountUpdateModel);
    Task<ResponseModel> UpdatePatch(Guid id, JsonPatchDocument<Account> patchDoc);
    Task<ResponseModel> Delete(Guid id);
    Task<ResponseModel> Restore(Guid id);
}