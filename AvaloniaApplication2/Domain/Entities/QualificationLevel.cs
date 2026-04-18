namespace AvaloniaApplication2.Domain.Entities;

public sealed class QualificationLevel : AuditableEntity
{
    public string name { get; set; } = string.Empty;

    public int sortOrder { get; set; }

    public ICollection<MasterProfile> masterProfiles { get; set; } = new List<MasterProfile>();

    public ICollection<QualificationApplication> qualificationApplications { get; set; } = new List<QualificationApplication>();
}
