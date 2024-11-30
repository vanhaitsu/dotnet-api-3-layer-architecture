using System.ComponentModel.DataAnnotations;

namespace Services.Models.ConversationModels;

public class ConversationAddModel
{
    [StringLength(50)] public string? Name { get; set; }
    public string? Image { get; set; }
    [Required] public List<Guid> AccountIds { get; set; } = null!;
}