using System.ComponentModel.DataAnnotations;

namespace Services.Models.AccountModels;

public class AccountEmailModel
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = null!;
}