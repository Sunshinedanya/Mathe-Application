namespace AvaloniaApplication2.Domain.Entities;

public sealed class Wallet : AuditableEntity
{
    public int userId { get; set; }

    public ApplicationUser? user { get; set; }

    public string currencyCode { get; set; } = "RUB";

    public ICollection<BalanceTopUp> balanceTopUps { get; set; } = new List<BalanceTopUp>();
}
