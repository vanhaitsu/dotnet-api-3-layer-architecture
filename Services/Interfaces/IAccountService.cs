using Microsoft.AspNetCore.JsonPatch;
using Repositories.Entities;
using Services.Models.AccountModels;
using Services.Models.ResponseModels;

namespace Services.Interfaces;

public interface IAccountService
{
    Task<ResponseModel> SignUp(AccountSignUpModel accountSignUpModel);
    Task<ResponseModel> SignIn(AccountSignInModel accountSignInModel);
    Task<ResponseModel> RefreshToken(AccountRefreshTokenModel accountRefreshTokenModel);
    Task<ResponseModel> RevokeTokens(AccountEmailModel accountEmailModel);
    Task<ResponseModel> VerifyEmail(string email, string verificationCode);
    Task<ResponseModel> ResendVerificationEmail(AccountEmailModel accountEmailModel);
    Task<ResponseModel> ChangePassword(AccountChangePasswordModel accountChangePasswordModel);
    Task<ResponseModel> ForgotPassword(AccountEmailModel accountEmailModel);
    Task<ResponseModel> ResetPassword(AccountResetPasswordModel accountResetPasswordModel);
    Task<ResponseModel> AddRange(List<AccountSignUpModel> accountSignUpModels);
    Task<ResponseModel> Get(string idOrUsername);
    Task<ResponseModel> GetAll(AccountFilterModel accountFilterModel);
    Task<ResponseModel> UpdatePut(Guid id, AccountUpdateModel accountUpdateModel);
    Task<ResponseModel> UpdatePatch(Guid id, JsonPatchDocument<Account> patchDoc);
    Task<ResponseModel> Delete(Guid id);
    Task<ResponseModel> Restore(Guid id);
}