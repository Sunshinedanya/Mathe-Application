namespace AvaloniaApplication2.Domain.Entities;

public sealed class UserRole : AuditableEntity
{
    public int userId { get; set; }

    public ApplicationUser? user { get; set; }

    public int roleId { get; set; }

    public Role? role { get; set; }
}
