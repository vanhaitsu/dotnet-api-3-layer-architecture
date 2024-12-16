using System.ComponentModel.DataAnnotations;
using Repositories.Enums;

namespace Services.Models.AccountModels;

public class AccountUpdateRolesModel
{
    [Required] public List<Role> Roles { get; set; } = null!;
}