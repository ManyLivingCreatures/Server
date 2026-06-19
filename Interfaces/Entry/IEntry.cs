using GameSchema.Schemas.Request;
using GameSchema.Schemas.Response;

namespace Interfaces.Entry;

[Alias("Interfaces.Entry.IEntry")]
public interface IEntry : IGrainWithGuidKey
{
    [Alias("SubscribeResponsePackets")]
    Task SubscribeEntryObserver(IEntryObserver observer);
    [Alias("UnsubscribeResponsePackets")]
    Task UnsubscribeEntryObserver(IEntryObserver observer);
    [Alias("NotifyObservers")]
    Task NotifyObservers(string connectionId, BaseResponsePacket message);
    [Alias("Initialize")]
    Task Initialize(IEntryObserver observer);
    [Alias("Uninitialize")]
    Task Uninitialize();
    [Alias("UpdateObserver")]
    Task UpdateObserver();
    [Alias("RequestPacketHandler")]
    Task RequestPacketHandler(string connectionId, BaseRequestPacket packet);
}