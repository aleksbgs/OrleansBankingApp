using System.Runtime.Serialization;

namespace Orleans.Client.Contracts;
[DataContract]
public record Credit
{
    [DataMember]
    public decimal amount { get; init; }
}