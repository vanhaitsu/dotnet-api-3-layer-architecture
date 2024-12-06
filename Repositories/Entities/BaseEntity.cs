namespace Repositories.Entities;

public class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? CreatedById { get; set; }
    public DateTime? ModificationDate { get; set; }
    public Guid? ModifiedById { get; set; }
    public DateTime? DeletionDate { get; set; }
    public Guid? DeletedById { get; set; }
    public bool IsDeleted { get; set; } = false;
}