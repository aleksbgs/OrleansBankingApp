using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Grains.Abstractions;
using Orleans.Grains.State;
using Orleans.Transactions.Abstractions;

namespace Orleans.Grains.Grains;

[Reentrant]
public class AtmGrain : Grain, IAtmGrain,IIncomingGrainCallFilter
{
    private readonly ILogger<AtmGrain> _logger;
    private readonly ITransactionalState<AtmState> _atmTransactionalState;


    public AtmGrain(ILogger<AtmGrain> logger,[TransactionalState("atm")] ITransactionalState<AtmState> atmTransactionalState)
    {
        _logger = logger;
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

    public async Task Invoke(IIncomingGrainCallContext context)
    {
        _logger.LogInformation(
            $"Incoming Atm grain filter: Received grain on '{context.Grain}' to '{context.MethodName}' method");
        await context.Invoke();
    }
}