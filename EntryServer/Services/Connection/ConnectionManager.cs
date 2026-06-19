using System.Threading.Channels;
using EntryServer.Services.Lifecycle;
using Interfaces.Entry;
using GameSchema.Schemas.Request;
using GameSchema.Schemas.Response;
using ServerCommon.Observer;

namespace EntryServer.Services.Connection;

public readonly struct Connection
{
    public string ConnectionId { get; init; }
    public Channel<BaseRequestPacket> RequestChannel { get; init; }
    public Channel<BaseResponsePacket> ResponseChannel { get; init; }
}

public class ConnectionManager(
    ILogger<ConnectionManager> logger,
    IClusterClient clusterClient,
    IClusterClientLifecycleTaskCompletionSources _orleansReadySource
) : IHostedService
{
    readonly TaskCompletionSource<IEntryManager> _entryManager = new();
    readonly TaskCompletionSource<IEntry> _entry = new();
    readonly Dictionary<string, Task> _handlers = [];
    readonly Dictionary<string, Connection> _connections = [];
    public async Task StartHandleAsync(string connectionId, Channel<BaseRequestPacket> requestChannel, Channel<BaseResponsePacket> responseChannel)
    {
        var connection = new Connection
        {
            ConnectionId = connectionId,
            RequestChannel = requestChannel,
            ResponseChannel = responseChannel
        };
        _connections.Add(connectionId, connection);
        var task = RequestPacketHandler(connection);
        _handlers.Add(connectionId, task);
    }

    public async Task StopHandleAsync(string connectionId)
    {
        // Per conn tell grain closed

        _handlers.Remove(connectionId);
        _connections.Remove(connectionId);
    }

    public async Task RequestPacketHandler(Connection connection)
    {
        var entry = await _entry.Task;
        logger.LogInformation("Starting request handler for connection {}", connection.ConnectionId);
        await foreach (var packet in connection.RequestChannel.Reader.ReadAllAsync())
        {
            logger.LogInformation("Rev {}", packet);
            await entry.RequestPacketHandler(connection.ConnectionId, packet);
        }

    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _orleansReadySource.Actived.Task;
        _entryManager.SetResult(clusterClient.GetGrain<IEntryManager>(0));
        var observerRef = clusterClient.CreateObjectReference<IEntryObserver>(new EntryObserver(EntryObserverHandler));
        _entry.SetResult(clusterClient.GetGrain<IEntry>(await (await _entryManager.Task).NewEntryGrain(observerRef)));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_entry.Task.IsCompleted)
        {
            await (await _entryManager.Task).DisposeEntryGrain((await _entry.Task).GetPrimaryKey());
        }

        foreach (var connection in _connections.Values)
        {
            connection.RequestChannel.Writer.TryComplete();
            connection.ResponseChannel.Writer.TryComplete();
        }

        await Task.WhenAll(_handlers.Values);
    }

    public async Task EntryObserverHandler(string connectionId, BaseResponsePacket packet)
    {
        if (_connections.TryGetValue(connectionId, out var connection))
        {
            await connection.ResponseChannel.Writer.WriteAsync(packet);
        }
        else
        {
            logger.LogWarning("Received response packet for unknown connection ID: {ConnectionId}", connectionId);
        }
    }
}