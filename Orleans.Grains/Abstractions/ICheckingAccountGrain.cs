namespace Orleans.Grains.Abstractions;

public interface ICheckingAccountGrain: IGrainWithGuidKey
{
    Task Initialize(decimal openingBalance);
    Task <decimal> GetBalance();
}