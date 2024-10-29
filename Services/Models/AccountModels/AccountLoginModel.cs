using System.ComponentModel.DataAnnotations;

namespace Services.Models.AccountModels;

public class AccountLoginModel
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = null!;
}