using AvaloniaApplication2.Domain.Enums;

namespace AvaloniaApplication2.Domain.Entities;

public sealed class QualificationApplication : AuditableEntity
{
    public int masterProfileId { get; set; }

    public MasterProfile? masterProfile { get; set; }

    public int requestedLevelId { get; set; }

    public QualificationLevel? requestedLevel { get; set; }

    public int? reviewedByUserId { get; set; }

    public ApplicationUser? reviewedByUser { get; set; }

    public QualificationApplicationStatus status { get; set; } = QualificationApplicationStatus.Pending;

    public string comment { get; set; } = string.Empty;

    public DateTime? reviewedAt { get; set; }
}
