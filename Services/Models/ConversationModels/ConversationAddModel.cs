using System.ComponentModel.DataAnnotations;

namespace Services.Models.ConversationModels;

public class ConversationAddModel
{
    [Required] public Guid RecipientId { get; set; }
}