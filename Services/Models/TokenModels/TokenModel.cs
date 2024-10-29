namespace Services.Models.TokenModels;

public class TokenModel
{
    public Guid DeviceId { get; set; }
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}