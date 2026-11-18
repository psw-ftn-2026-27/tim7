using Shared.Domain;
using Shared.Domain.Exceptions;

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

    public void Deposit(Guid administratorId, int amount)
    {
        _transactions.Add(new Transaction(amount, administratorId));
    }

    public void ReverseDeposit(Guid transactionId, DateTime now)
    {
        var transaction = _transactions.FirstOrDefault(transaction => transaction.Id == transactionId);
        if (transaction is null)
        {
            throw new DomainException("The deposit does not belong to this wallet.");
        }

        if (!transaction.IsReversed && Balance() - transaction.Amount < 0)
        {
            throw new DomainException("The deposit cannot be reversed because the balance would become negative.");
        }

        transaction.Reverse(now);
    }

    public int Balance()
    {
        return _transactions.Where(transaction => !transaction.IsReversed).Sum(transaction => transaction.Amount);
    }
}
