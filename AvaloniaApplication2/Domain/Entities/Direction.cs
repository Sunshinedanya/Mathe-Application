namespace AvaloniaApplication2.Domain.Entities;

public sealed class Direction : AuditableEntity
{
    public string name { get; set; } = string.Empty;

    public string description { get; set; } = string.Empty;

    public ICollection<Collection> collections { get; set; } = new List<Collection>();
}
