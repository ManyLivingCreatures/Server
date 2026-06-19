using Interfaces;
using Interfaces.Entry;
using Orleans;

namespace ServerCommon.Services.Entry;

public interface IEntryService
{
}

public class EntryService(
    IClusterClient clusterClient
) : IEntryService
{
    readonly IEntry pingGrain = clusterClient.GetGrain<IEntry>(new Guid());
}