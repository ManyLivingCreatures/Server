using System.Text.Json.Serialization;
using GameSchema.Schemas.Request;

namespace GameSchema.Schemas.Response;

[GenerateSerializer]
[JsonDerivedType(typeof(TestResponsePacket), typeDiscriminator: (int)ResponsePacketType.Test)]
[Alias("SchemaCommon.Schemas.Response.BaseResponsePacket")]
public abstract record BaseResponsePacket {}

[GenerateSerializer]
public record TestResponsePacket : BaseResponsePacket
{
    [Id(0)]
    public required string Message { get; init; }
}
