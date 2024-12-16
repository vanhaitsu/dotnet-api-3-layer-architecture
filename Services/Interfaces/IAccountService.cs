using Services.Models.AccountModels;
using Services.Models.ResponseModels;

namespace Services.Interfaces;

public interface IAccountService
{
    Task<ResponseModel> SignUp(AccountSignUpModel accountSignUpModel);
    Task<ResponseModel> SignIn(AccountSignInModel accountSignInModel);
    Task<ResponseModel> SignInGoogle(string code);
    Task<ResponseModel> RefreshToken(AccountRefreshTokenModel accountRefreshTokenModel);
    Task<ResponseModel> RevokeTokens(AccountEmailModel accountEmailModel);
    Task<ResponseModel> VerifyEmail(string email, string verificationCode);
    Task<ResponseModel> ResendVerificationEmail(AccountEmailModel accountEmailModel);
    Task<ResponseModel> ChangePassword(AccountChangePasswordModel accountChangePasswordModel);
    Task<ResponseModel> ForgotPassword(AccountEmailModel accountEmailModel);
    Task<ResponseModel> ResetPassword(AccountResetPasswordModel accountResetPasswordModel);
    Task<ResponseModel> AddRange(AccountAddRangeModel accountAddRangeModel);
    Task<ResponseModel> Get(string idOrUsername);
    Task<ResponseModel> GetAll(AccountFilterModel accountFilterModel);
    Task<ResponseModel> Update(Guid id, AccountUpdateModel accountUpdateModel);
    Task<ResponseModel> UpdateRoles(Guid id, AccountUpdateRolesModel accountUpdateRolesModel);
    Task<ResponseModel> Delete(Guid id);
    Task<ResponseModel> Restore(Guid id);
}