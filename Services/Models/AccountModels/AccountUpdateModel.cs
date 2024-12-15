using System.ComponentModel.DataAnnotations;
using Repositories.Enums;
using Services.Models.AccountModels.Validations;

namespace Services.Models.AccountModels;

public class AccountUpdateModel
{
    [Required] [StringLength(50)] public string FirstName { get; set; } = null!;
    [Required] [StringLength(50)] public string LastName { get; set; } = null!;
    [Required] [StringLength(50)] public string Username { get; set; } = null!;

    [Required]
    [EnumDataType(typeof(Gender))]
    public Gender Gender { get; set; }

    [Required] [DateOfBirthValidation] public DateOnly? DateOfBirth { get; set; }
    [Required] [Phone] [StringLength(15)] public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Image { get; set; }
}