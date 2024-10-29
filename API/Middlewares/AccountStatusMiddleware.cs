using Newtonsoft.Json;
using Repositories.Interfaces;

namespace API.Middlewares;

public class AccountStatusMiddleware : IMiddleware
{
    private readonly IClaimService _claimService;
    private readonly IUnitOfWork _unitOfWork;

    public AccountStatusMiddleware(IUnitOfWork unitOfWork,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork;
        _claimService = claimService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var currentUserId = _claimService.GetCurrentUserId;
        if (currentUserId != null)
        {
            var account = await _unitOfWork.AccountRepository.GetAsync(currentUserId.Value);
            if (account != null && account.IsDeleted)
            {
                var response = new
                {
                    isDeleted = true,
                    message = "Account has been deleted"
                };

                var jsonResponse = JsonConvert.SerializeObject(response);
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(jsonResponse);

                return;
            }
        }

        await next(context);
    }
}