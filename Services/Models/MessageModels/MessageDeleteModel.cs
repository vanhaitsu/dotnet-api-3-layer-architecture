using System.ComponentModel.DataAnnotations;
using Repositories.Enums;

namespace Services.Models.MessageModels;

public class MessageDeleteModel
{
    [Required] public MessageDeleteType MessageDeleteType { get; set; }
}