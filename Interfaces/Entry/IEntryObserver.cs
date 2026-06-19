namespace Interfaces.Entry;

using GameSchema.Schemas.Response;

[Alias("Interfaces.Entry.IEntryObserver")]
public interface IEntryObserver : IGrainObserver
{
    [Alias("ReceiveEntryMessage")]
    Task ReceiveResponsePacket(string connectionId, BaseResponsePacket packet);
}