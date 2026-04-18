using System.Collections.Generic;

namespace AvaloniaApplication2.Domain.Entities;

public sealed class PaymentCard : AuditableEntity
{
    public int userId { get; set; }

    public ApplicationUser? user { get; set; }

    public string maskedPan { get; set; } = string.Empty;

    public string paymentToken { get; set; } = string.Empty;

    public string holderName { get; set; } = string.Empty;

    public int expMonth { get; set; }

    public int expYear { get; set; }

    public bool isDefault { get; set; }

    public ICollection<BalanceTopUp> balanceTopUps { get; set; } = new List<BalanceTopUp>();
}
