using System.ComponentModel.DataAnnotations;

namespace Services.Models.AccountModels;

public class AccountAddRangeModel
{
    [Required] public List<AccountSignUpModel> Accounts { get; set; } = null!;
}