using AvaloniaApplication2.Domain.Enums;

namespace AvaloniaApplication2.Domain.Entities;

public sealed class Appointment : AuditableEntity
{
    public int clientUserId { get; set; }

    public ApplicationUser? clientUser { get; set; }

    public int masterServiceId { get; set; }

    public MasterService? masterService { get; set; }

    public DateTime startsAt { get; set; }

    public DateTime endsAt { get; set; }

    public AppointmentStatus status { get; set; } = AppointmentStatus.Pending;

    public string clientComment { get; set; } = string.Empty;

    public Review? review { get; set; }
}
