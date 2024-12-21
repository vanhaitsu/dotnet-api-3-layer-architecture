using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Repositories.Enums;
using Services.Models.AccountModels.Validations;

namespace Services.Models.AccountModels;

public class AccountUpdateModel
{
    [Required] [StringLength(50)] public string FirstName { get; set; } = null!;
    [Required] [StringLength(50)] public string LastName { get; set; } = null!;

    [Required]
    [RegularExpression(@"^[a-zA-Z0-9_]*$",
        ErrorMessage = "Username can only contain alphanumeric characters and underscores.")]
    [StringLength(50)]
    public string Username { get; set; } = null!;

    [Required]
    [EnumDataType(typeof(Gender))]
    public Gender Gender { get; set; }

    [Required] [DateOfBirthValidation] public DateOnly DateOfBirth { get; set; }
    [Required] [Phone] [StringLength(15)] public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public IFormFile? Image { get; set; }
}