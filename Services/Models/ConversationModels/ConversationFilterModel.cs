using Repositories.Common;
using Services.Common;

namespace Services.Models.ConversationModels;

public class ConversationFilterModel : FilterParameter
{
    protected override int MinPageSize { get; set; } = Constant.ConversationMinPageSize;
    // protected override int MaxPageSize { get; set; } = Constant.;
}