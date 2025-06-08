namespace Orleans.Grains.State;
[GenerateSerializer]
public class TransferState
{
    [Id(0)] 
    public int TransferCount { get; set; }
}