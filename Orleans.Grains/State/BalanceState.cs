namespace Orleans.Grains.State;

[GenerateSerializer]
public record BalanceState
{
    [Id(0)]
    public decimal Balance { get; set; }
}