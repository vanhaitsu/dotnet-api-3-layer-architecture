using System.ComponentModel.DataAnnotations;

namespace Services.Models.AccountModels;

public class AccountChangePasswordModel
{
    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string OldPassword { get; set; } = null!;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string NewPassword { get; set; } = null!;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = null!;
}