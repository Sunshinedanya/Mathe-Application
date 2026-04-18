using AvaloniaApplication2.Domain.Enums;

namespace AvaloniaApplication2.Domain.Entities;

public sealed class BalanceTopUp : AuditableEntity
{
    public int walletId { get; set; }

    public Wallet? wallet { get; set; }

    public int cardId { get; set; }

    public PaymentCard? card { get; set; }

    public decimal amount { get; set; }

    public TopUpStatus status { get; set; } = TopUpStatus.Pending;

    public string? externalPaymentId { get; set; }

    public DateTime? completedAt { get; set; }
}
