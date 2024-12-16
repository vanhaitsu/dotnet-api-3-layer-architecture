using System.ComponentModel.DataAnnotations;

namespace Services.Models.MessageModels;

public class MessageAddModel
{
    // TODO: Upload files
    [Required] public string Content { get; set; } = null!;
}