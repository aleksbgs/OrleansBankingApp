namespace Orleans.Grains.Abstractions;

public interface ICheckingAccountGrain : IGrainWithGuidKey
{
    [Transaction(TransactionOption.Create)]
    Task Initialize(decimal openingBalance);
    [Transaction(TransactionOption.Create)]
    Task<decimal> GetBalance();
    [Transaction(TransactionOption.CreateOrJoin)]
    Task Debit(decimal amount);
    [Transaction(TransactionOption.CreateOrJoin)]
    Task Credit(decimal amount);

    Task AddRecurringPayment(Guid id, decimal amount, int reccursEveryMinutes);
}