using System.Text.Json.Serialization;

namespace GameSchema.Schemas.Request;

[GenerateSerializer]
[JsonDerivedType(typeof(TestRequestPacket), typeDiscriminator: (int)RequestPacketType.Test)]
[Alias("SchemaCommon.Schemas.Request.BaseRequestPacket")]
public abstract record BaseRequestPacket {}

[GenerateSerializer]
public record TestRequestPacket : BaseRequestPacket
{
    [Id(0)]
    public required string Message { get; init; }
}