namespace AvaloniaApplication2.Domain.Entities;

public sealed class MasterService : AuditableEntity
{
    public int masterProfileId { get; set; }

    public MasterProfile? masterProfile { get; set; }

    public int serviceId { get; set; }

    public ShopService? service { get; set; }

    public decimal? customPrice { get; set; }

    public bool isActive { get; set; } = true;

    public ICollection<Appointment> appointments { get; set; } = new List<Appointment>();
}
