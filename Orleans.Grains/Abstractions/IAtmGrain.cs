namespace Orleans.Grains.Abstractions;

public interface IAtmGrain:IGrainWithGuidKey
{
    public Task Initialize(decimal openingBalance);
    public Task Withdraw(Guid checkingAccountId, decimal amount);
}