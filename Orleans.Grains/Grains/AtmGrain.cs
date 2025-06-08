using Orleans.Grains.Abstractions;
using Orleans.Grains.State;

namespace Orleans.Grains.Grains;

public class AtmGrain : Grain, IAtmGrain
{
    private readonly IPersistentState<AtmState> _atm;

    public AtmGrain([PersistentState("atm", "tableStorage")] IPersistentState<AtmState> atm)
    {
        _atm = atm;
    }

    public async Task Initialize(decimal openingBalance)
    {
        _atm.State.Balance = openingBalance;
        _atm.State.Id = this.GetGrainId().GetGuidKey();
    }

    public async Task Withdraw(Guid checkingAccountId, decimal amount)
    {
        var checkingAccountGrain = this.GrainFactory.GetGrain<ICheckingAccountGrain>(checkingAccountId);
        await checkingAccountGrain.Debit(amount);

        var currentAtmBalance = _atm.State.Balance;
        var updatedAtmBalance = currentAtmBalance - amount;
        _atm.State.Balance = updatedAtmBalance;
        await _atm.WriteStateAsync();
    }
}