using Orleans.Concurrency;
using Orleans.Grains.Abstractions;
using Orleans.Grains.State;
using Orleans.Transactions.Abstractions;

namespace Orleans.Grains.Grains;

[Reentrant]
public class AtmGrain : Grain, IAtmGrain
{
    private readonly ITransactionalState<AtmState> _atmTransactionalState;


    public AtmGrain([TransactionalState("atm")] ITransactionalState<AtmState> atmTransactionalState)
    {
        _atmTransactionalState = atmTransactionalState;
    }

    public async Task Initialize(decimal openingBalance)
    {
        await _atmTransactionalState.PerformUpdate(state =>
        {
            state.Balance = openingBalance;
            state.Id = this.GetGrainId().GetGuidKey();
        });

    }

    public async Task Withdraw(Guid checkingAccountId, decimal amount)
    {
        _atmTransactionalState.PerformUpdate(state =>
        {
            var currentBalance = state.Balance;
            var updatedBalance = currentBalance - amount;
            state.Balance = updatedBalance;

        });
    }

    public async Task<decimal> GetBalance()
    {
        return await _atmTransactionalState.PerformRead((state) => state.Balance);
    }
}