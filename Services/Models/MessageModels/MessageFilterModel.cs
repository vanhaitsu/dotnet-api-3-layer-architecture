using Repositories.Common;
using Services.Common;

namespace Services.Models.MessageModels;

public class MessageFilterModel : FilterParameter
{
    // protected override int MinPageSize { get; set; } = Constant.;
    protected override int MaxPageSize { get; set; } = Constant.MessageMaxPageSize;
}