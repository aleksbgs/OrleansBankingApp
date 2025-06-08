using System.Runtime.Serialization;

namespace Orleans.Client.Contracts;
[DataContract]
public record Debit
{
    [DataMember]
    public decimal amount { get; init; }
    
    
}