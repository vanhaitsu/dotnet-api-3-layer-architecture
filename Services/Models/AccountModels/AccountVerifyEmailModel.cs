using System.ComponentModel.DataAnnotations;

namespace Services.Models.AccountModels;

public class AccountVerifyEmailModel
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = null!;

    [Required] public string VerificationCode { get; set; } = null!;
}