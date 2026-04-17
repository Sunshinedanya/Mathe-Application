namespace AvaloniaApplication2.Domain.Entities;

public sealed class Role : AuditableEntity
{
    public string code { get; set; } = string.Empty;

    public string name { get; set; } = string.Empty;

    public ICollection<UserRole> userRoles { get; set; } = new List<UserRole>();
}
