using System.ComponentModel.DataAnnotations;
using Repositories.Enums;

namespace Services.Models.AccountModels;

public class AccountRegisterModel
{
    [Required] [StringLength(50)] public string FirstName { get; set; } = null!;
    [Required] [StringLength(50)] public string LastName { get; set; } = null!;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = null!;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = null!;

    [Required]
    [EnumDataType(typeof(Gender))]
    public Gender Gender { get; set; }

    [Required] public DateOnly DateOfBirth { get; set; }

    [Required] [Phone] [StringLength(15)] public string PhoneNumber { get; set; } = null!;

    public string? Address { get; set; }

    [EnumDataType(typeof(Role))] public Role? Role { get; set; }
}