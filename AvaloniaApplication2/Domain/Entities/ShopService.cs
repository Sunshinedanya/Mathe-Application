namespace AvaloniaApplication2.Domain.Entities;

public sealed class ShopService : AuditableEntity
{
    public int collectionId { get; set; }

    public Collection? collection { get; set; }

    public string name { get; set; } = string.Empty;

    public string description { get; set; } = string.Empty;

    public int durationMinutes { get; set; }

    public decimal basePrice { get; set; }

    public bool isActive { get; set; } = true;

    public string? imagePath { get; set; }

    public ICollection<MasterService> masterServices { get; set; } = new List<MasterService>();
}
