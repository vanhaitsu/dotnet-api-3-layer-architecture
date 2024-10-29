namespace Services.Models.ResponseModel;

public class ResponseModel
{
    public bool Status { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}