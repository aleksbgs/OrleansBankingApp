using System.Transactions;
using Orleans.Concurrency;
using Orleans.Grains.Abstractions;
using Orleans.Grains.State;
using Orleans.Transactions.Abstractions;

namespace Orleans.Grains.Grains;

[Reentrant]
public class CheckingAccountGrain : Grain, ICheckingAccountGrain, IRemindable
{
    private readonly ITransactionClient _transactionClient;
    private readonly ITransactionalState<BalanceState> _balanceTransactionalState;
    private readonly IPersistentState<CheckingAccountState> _checkingAccountState;


    public CheckingAccountGrain(
        ITransactionClient transactionClient,
        [TransactionalState("balance")] ITransactionalState<BalanceState> balanceTransactionalState,
        [PersistentState("checkingAccount", "blobStorage")]
        IPersistentState<CheckingAccountState> checkingAccountState
    )
    {
        _transactionClient = transactionClient;
        _balanceTransactionalState = balanceTransactionalState;
        _checkingAccountState = checkingAccountState;
    }

    public async Task Initialize(decimal openingBalance)
    {
        _checkingAccountState.State.OpenedAtUtc = DateTime.UtcNow;
        _checkingAccountState.State.AccountType = "Default";
        _checkingAccountState.State.AccountId = this.GetGrainId().GetGuidKey();

        await _balanceTransactionalState.PerformUpdate(state => { state.Balance = openingBalance; });
        await _checkingAccountState.WriteStateAsync();
    }

    public async Task<decimal> GetBalance()
    {
        return await _balanceTransactionalState.PerformRead(state => state.Balance);
    }

    public async Task Debit(decimal amount)
    {
        await _balanceTransactionalState.PerformUpdate(state =>
        {
            var currentBalance = state.Balance;
            var newBalance = currentBalance - amount;
            state.Balance = newBalance;
        });
    }

    public async Task Credit(decimal amount)
    {
        await _balanceTransactionalState.PerformUpdate(state =>
        {
            var currentBalance = state.Balance;
            var newBalance = currentBalance + amount;
            state.Balance = newBalance;
        });

    }

    public async Task AddRecurringPayment(Guid id, decimal amount, int reccursEveryMinutes)
    {
        _checkingAccountState.State.RecurringPayments.Add(new RecurringPayment
        {
            PaymentId = id,
            PaymentAmount = amount,
            OccursEveryMinutes = reccursEveryMinutes
        });
        await _checkingAccountState.WriteStateAsync();
        await this.RegisterOrUpdateReminder($"RecurringPayment:::{id}", TimeSpan.FromMinutes(reccursEveryMinutes),
            TimeSpan.FromMinutes(reccursEveryMinutes));

    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (reminderName.StartsWith("RecurringPayment::"))
        {
            var recurringPaymentId = Guid.Parse(reminderName.Split(":::").Last());
            var recurringPayment =
                _checkingAccountState.State.RecurringPayments.Single(p => p.PaymentId == recurringPaymentId);

            await _transactionClient.RunTransaction(TransactionOption.Create,
                async () => { await Debit(recurringPayment.PaymentAmount); });
        }
    }
}