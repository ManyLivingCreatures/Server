using Interfaces.Entry;
using GameSchema.Schemas.Response;

namespace ServerCommon.Observer;

public class EntryObserver(Func<string, BaseResponsePacket, Task> onReceive) : IEntryObserver
{
    public async Task ReceiveResponsePacket(string connectionId, BaseResponsePacket packet)
    {
        await onReceive(connectionId, packet);
    }
}