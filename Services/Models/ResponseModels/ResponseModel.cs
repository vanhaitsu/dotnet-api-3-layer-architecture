using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Services.Models.ResponseModels;

public class ResponseModel
{
    [JsonIgnore] public int Code { get; set; } = StatusCodes.Status200OK;
    public bool Status => Code >= StatusCodes.Status200OK && Code < 300;
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}