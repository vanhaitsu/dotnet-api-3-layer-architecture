using Microsoft.AspNetCore.Http;

namespace Services.Models.ResponseModels;

public class ResponseModel
{
    public int StatusCode { get; set; } = StatusCodes.Status200OK;
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}