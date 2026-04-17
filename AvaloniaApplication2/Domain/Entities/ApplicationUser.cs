namespace AvaloniaApplication2.Domain.Entities;

public sealed class ApplicationUser : AuditableEntity
{
    public string email { get; set; } = string.Empty;

    public string passwordHash { get; set; } = string.Empty;

    public string fullName { get; set; } = string.Empty;

    public string phone { get; set; } = string.Empty;

    public bool isActive { get; set; } = true;

    public Wallet? wallet { get; set; }

    public MasterProfile? masterProfile { get; set; }

    public ICollection<UserRole> userRoles { get; set; } = new List<UserRole>();

    public ICollection<PaymentCard> paymentCards { get; set; } = new List<PaymentCard>();

    public ICollection<Appointment> clientAppointments { get; set; } = new List<Appointment>();

    public ICollection<Review> reviews { get; set; } = new List<Review>();

    public ICollection<QualificationApplication> reviewedQualificationApplications { get; set; } = new List<QualificationApplication>();
}
