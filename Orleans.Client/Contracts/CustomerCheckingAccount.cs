using System.Runtime.Serialization;

namespace Orleans.Client.Contracts;
[DataContract]
public record CustomerCheckingAccount
{
    [DataMember]
    public Guid AccountId { get; init; }
}