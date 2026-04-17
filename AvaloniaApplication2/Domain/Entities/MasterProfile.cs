namespace AvaloniaApplication2.Domain.Entities;

public sealed class MasterProfile : AuditableEntity
{
    public int userId { get; set; }

    public ApplicationUser? user { get; set; }

    public int qualificationLevelId { get; set; }

    public QualificationLevel? qualificationLevel { get; set; }

    public string bio { get; set; } = string.Empty;

    public int experienceYears { get; set; }

    public bool isActive { get; set; } = true;

    public ICollection<MasterService> masterServices { get; set; } = new List<MasterService>();

    public ICollection<QualificationApplication> qualificationApplications { get; set; } = new List<QualificationApplication>();
}
