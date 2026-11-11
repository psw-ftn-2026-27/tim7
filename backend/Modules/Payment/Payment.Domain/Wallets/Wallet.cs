using Shared.Domain;

namespace Payment.Domain.Wallets;

public sealed class Wallet : AggregateRoot
{
    private readonly List<Transaction> _transactions = [];

    public Guid UserId { get; private set; }
    public IReadOnlyList<Transaction> Transactions => _transactions;

    private Wallet() { }

    public Wallet(Guid userId) : base(Guid.NewGuid())
    {
        UserId = userId;
    }

    public int Balance()
    {
        return _transactions.Where(transaction => !transaction.IsReversed).Sum(transaction => transaction.Amount);
    }
}
