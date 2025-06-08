using Orleans.Grains.Abstractions;
using Orleans.Grains.State;

namespace Orleans.Grains.Grains;

public class CheckingAccountGrain : Grain, ICheckingAccountGrain, IRemindable
{
    private readonly IPersistentState<BalanceState> _balanceState;
    private readonly IPersistentState<CheckingAccountState> _checkingAccountState;

    public CheckingAccountGrain(
        [PersistentState("balance", "tableStorage")]
        IPersistentState<BalanceState> balanceState,
        [PersistentState("checkingAccount", "blobStorage")]
        IPersistentState<CheckingAccountState> checkingAccountState
    )
    {
        _balanceState = balanceState;
        _checkingAccountState = checkingAccountState;
    }

    public async Task Initialize(decimal openingBalance)
    {
        _checkingAccountState.State.OpenedAtUtc = DateTime.UtcNow;
        _checkingAccountState.State.AccountType = "Default";
        _checkingAccountState.State.AccountId = this.GetGrainId().GetGuidKey();
        _balanceState.State.Balance = openingBalance;
        await _balanceState.WriteStateAsync();
        await _checkingAccountState.WriteStateAsync();
    }

    public async Task<decimal> GetBalance()
    {
        return _balanceState.State.Balance;
    }

    public async Task Debit(decimal amount)
    {
        var currentBalance = _balanceState.State.Balance;
        var newBalance = currentBalance - amount;
        _balanceState.State.Balance = newBalance;
        await _balanceState.WriteStateAsync();
    }

    public async Task Credit(decimal amount)
    {
        var currentBalance = _balanceState.State.Balance;
        var newBalance = currentBalance + amount;
        _balanceState.State.Balance = newBalance;
        await _balanceState.WriteStateAsync();
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

            await Debit(recurringPayment.PaymentAmount);

        }
    }
}