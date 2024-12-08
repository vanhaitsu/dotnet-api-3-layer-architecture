using Repositories.Common;
using Services.Common;

namespace Services.Models.ConversationModels;

public class ConversationFilterModel : FilterParameter
{
    public bool? IsArchived { get; set; } = false;
    
    // protected override int MinPageSize { get; set; } = Constant.;
    protected override int MaxPageSize { get; set; } = Constant.ConversationMaxPageSize;
}