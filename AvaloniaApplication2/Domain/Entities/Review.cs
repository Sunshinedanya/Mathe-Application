using AvaloniaApplication2.Domain.Enums;

namespace AvaloniaApplication2.Domain.Entities;

public sealed class Review : AuditableEntity
{
    public int appointmentId { get; set; }

    public Appointment? appointment { get; set; }

    public int authorUserId { get; set; }

    public ApplicationUser? authorUser { get; set; }

    public int rating { get; set; }

    public string text { get; set; } = string.Empty;

    public ReviewStatus status { get; set; } = ReviewStatus.Pending;
}
