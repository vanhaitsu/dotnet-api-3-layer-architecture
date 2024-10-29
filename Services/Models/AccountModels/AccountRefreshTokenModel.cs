namespace Services.Models.AccountModels;

public class AccountRefreshTokenModel
{
    public Guid? DeviceId { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}