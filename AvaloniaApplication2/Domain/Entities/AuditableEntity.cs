namespace AvaloniaApplication2.Domain.Entities;

public abstract class AuditableEntity
{
    public int Id { get; set; }

    public DateTime createdAt { get; set; } = DateTime.UtcNow;

    public DateTime lastModifiedAt { get; set; } = DateTime.UtcNow;
}
