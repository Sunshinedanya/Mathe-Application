namespace AvaloniaApplication2.Domain.Entities;

public sealed class Collection : AuditableEntity
{
    public int directionId { get; set; }

    public Direction? direction { get; set; }

    public string name { get; set; } = string.Empty;

    public string description { get; set; } = string.Empty;

    public ICollection<ShopService> services { get; set; } = new List<ShopService>();
}
