using System.Text.Json.Serialization;

namespace Services.Models.AccountModels.OAuth2;

public class GoogleUserInformationModel
{
    [JsonPropertyName("given_name")] public string FirstName { get; set; } = null!;
    [JsonPropertyName("family_name")] public string LastName { get; set; } = null!;
    [JsonPropertyName("email")] public string Email { get; set; } = null!;
    [JsonPropertyName("picture")] public string Image { get; set; } = null!;
}