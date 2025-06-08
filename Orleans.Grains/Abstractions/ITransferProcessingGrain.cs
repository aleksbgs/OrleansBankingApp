namespace Orleans.Grains.Abstractions;

public interface ITransferProcessingGrain: IGrainWithIntegerKey
{
    Task ProcessTransfer(Guid fromAccountId, Guid toAccountId, decimal amount);
}