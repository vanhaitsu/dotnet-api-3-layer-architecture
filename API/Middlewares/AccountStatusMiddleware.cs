using System.Text.Json;
using Repositories.Interfaces;
using Services.Models.ResponseModels;

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
        if (currentUserId.HasValue)
        {
            var account = await _unitOfWork.AccountRepository.GetAsync(currentUserId.Value);
            if (account != null && account.IsDeleted)
            {
                var response = new ResponseModel
                {
                    Code = StatusCodes.Status403Forbidden,
                    Message = "Your account has been deleted",
                    Data = new
                    {
                        IsDeleted = true
                    }
                };
                var jsonResponse = JsonSerializer.Serialize(response);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(jsonResponse);

                return;
            }
        }

        await next(context);
    }
}