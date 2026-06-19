using Orleans;

namespace Interfaces.Entry;

[Alias("Interfaces.Entry.IEntryManager")]
public interface IEntryManager : IGrainWithIntegerKey
{
    [Alias("Initialize")]
    Task Initialize();
    [Alias("Uninitialize")]
    Task Uninitialize();
    [Alias("NewEntryGrain")]
    Task<Guid> NewEntryGrain(IEntryObserver observer);

    [Alias("DisposeEntryGrain")]
    Task DisposeEntryGrain(Guid entryGrainId);
}