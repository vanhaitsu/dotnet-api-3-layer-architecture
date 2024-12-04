using Repositories.Entities;

namespace Repositories.Models.MessageModels;

public class LatestMessageModel : BaseEntity
{
    public string Message { get; set; } = null!;
    public Guid AccountId { get; set; }
    public string SenderFirstName { get; set; } = null!;
}