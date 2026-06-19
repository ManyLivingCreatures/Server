using System.Text.Json.Serialization;

namespace GameSchema;

public record TestMessage
{
    [JsonRequired]
    public string? Message { get; init; } = string.Empty;
}