using System.ComponentModel.DataAnnotations;

namespace Services.Models.MessageModels;

public class MessageAddModel
{
    [Required] public string Content { get; set; } = null!;
}