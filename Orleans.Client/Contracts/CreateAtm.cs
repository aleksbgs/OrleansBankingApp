using System.Runtime.Serialization;

namespace Orleans.Client.Contracts;
[DataContract]
public record CreateAtm
{
    [DataMember]
    public decimal InitialAtmCashBalance { get; init; }
}