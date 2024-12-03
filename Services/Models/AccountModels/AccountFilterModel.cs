using Repositories.Enums;
using Services.Common;

namespace Services.Models.AccountModels;

public class AccountFilterModel : FilterParameter
{
    public Gender? Gender { get; set; }
    public Role? Role { get; set; }

    // protected override int MinPageSize { get; set; } = Constant.;
    // protected override int MaxPageSize { get; set; } = Constant.;
}