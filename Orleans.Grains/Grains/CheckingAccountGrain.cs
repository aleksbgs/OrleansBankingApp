using Orleans.Grains.Abstractions;

namespace Orleans.Grains.Grains;

public class CheckingAccountGrain : ICheckingAccountGrain
{
    private decimal _balance;

    public async Task Initialize(decimal openingBalance)
    {
        _balance = openingBalance;
    }

    public async Task<decimal> GetBalance()
    {
        return _balance;
    }
}