using Interfaces.Entry;
using Microsoft.Extensions.Logging;
using Orleans.Utilities;
using GameSchema.Schemas.Request;
using GameSchema.Schemas.Response;

namespace Grains.Entry;

public class EntryGrain(
    ILogger<EntryGrain> logger
) : Grain, IEntry
{
    private readonly ObserverManager<IEntryObserver> _subscriptionManager = new(TimeSpan.FromMinutes(5), logger);
    private IEntryObserver? _observer;

    public Task Initialize(IEntryObserver observer)
    {
        _observer = observer;
        SubscribeEntryObserver(_observer);
        return Task.CompletedTask;
    }

    public Task NotifyObservers(string connectionId, BaseResponsePacket message)
    {
        _subscriptionManager.Notify(observer => observer.ReceiveResponsePacket(connectionId, message));
        return Task.CompletedTask;
    }

    public async Task RequestPacketHandler(string connectionId, BaseRequestPacket packet)
    {
        switch (packet)
        {
            case TestRequestPacket testRequest:
                logger.LogInformation("Received TestRequestPacket from connection {ConnectionId}", connectionId);
                var response = new TestResponsePacket
                {
                    Message = $"Test response for connection {connectionId} {testRequest.Message}"
                };
                await NotifyObservers(connectionId, response);
                break;
            default:
                logger.LogWarning("Received unknown packet type from connection {ConnectionId}: {PacketType}", connectionId, packet.GetType().Name);
                break;
        }
    }

    public Task SubscribeEntryObserver(IEntryObserver observer)
    {
        _subscriptionManager.Subscribe(observer, observer);
        return Task.CompletedTask;
    }

    public Task Uninitialize()
    {
        if (_observer != null)
        {
            UnsubscribeEntryObserver(_observer);
        }
        return Task.CompletedTask;
    }

    public Task UnsubscribeEntryObserver(IEntryObserver observer)
    {
        _subscriptionManager.Unsubscribe(observer);
        return Task.CompletedTask;
    }

    public Task UpdateObserver()
    {
        if (_observer != null)
        {
            _subscriptionManager.Subscribe(_observer, _observer);
        }
        return Task.CompletedTask;
    }
}