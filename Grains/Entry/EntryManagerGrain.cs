using Interfaces.Entry;

using Orleans;
namespace Grains.Entry;

[Serializable]
[GenerateSerializer]
[Alias("Grains.Entry.EntryManagerState")]
public record EntryManagerState
{
    [Id(0)]
    public HashSet<Guid> Observers { get; init; } = [];
}

public class EntryManagerGrain(
    [PersistentState("entryManager", "default")] IPersistentState<EntryManagerState> state
) : Grain, IEntryManager, IRemindable
{
    private IGrainReminder? _observerUpdateReminder;
    public async Task<Guid> NewEntryGrain(IEntryObserver observer)
    {
        var entryGrainId = Guid.NewGuid();
        var entryGrain = GrainFactory.GetGrain<IEntry>(entryGrainId);
        await entryGrain.Initialize(observer);
        state.State.Observers.Add(entryGrainId);
        await state.WriteStateAsync();
        return entryGrainId;
    }

    public async Task DisposeEntryGrain(Guid entryGrainId)
    {
        var entryGrain = GrainFactory.GetGrain<IEntry>(entryGrainId);
        await entryGrain.Uninitialize();
        state.State.Observers.Remove(entryGrainId);
        await state.WriteStateAsync();
    }

    public Task ReceiveReminder(string reminderName, TickStatus status)
    {
        return reminderName switch
        {
            "ObserverUpdateReminder" => UpdateObservers(),
            _ => Task.CompletedTask,
        };
    }

    async Task UpdateObservers()
    {
        foreach (var observerId in state.State.Observers)
        {
            var entryGrain = GrainFactory.GetGrain<IEntry>(observerId);
            await entryGrain.UpdateObserver();
        }
    }

    async Task SetupReminder()
    {
        _observerUpdateReminder = await this.RegisterOrUpdateReminder("ObserverUpdateReminder", TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
    }

    async Task RemoveReminder()
    {
        if (_observerUpdateReminder is not null)
        {
            await this.UnregisterReminder(_observerUpdateReminder);
            _observerUpdateReminder = null;
        }
    }

    public async Task Initialize()
    {
        await SetupReminder();
    }

    public async Task Uninitialize()
    {
        await RemoveReminder();
    }
}