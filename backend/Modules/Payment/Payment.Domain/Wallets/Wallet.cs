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
        if (administratorId == UserId)
        {
            throw new DomainException("An administrator cannot deposit to his own wallet.");
        }

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

    public void Spend(Guid entityId, int amount, DateTime now)
    {
        if (amount > Balance())
        {
            throw new DomainException("There is not enough EC in the wallet.");
        }

        _transactions.Add(new Transaction(TransactionKind.Purchase, entityId, amount, now));
    }

    public void Earn(Guid entityId, int amount, DateTime now)
    {
        _transactions.Add(new Transaction(TransactionKind.Earning, entityId, amount, now));
    }

    public int Balance()
    {
        return _transactions.Where(transaction => !transaction.IsReversed).Sum(transaction => transaction.Amount);
    }
}
