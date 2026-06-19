namespace EntryServer.Services.Lifecycle;

public class IClusterClientLifecycleTaskCompletionSources
{
    public TaskCompletionSource Actived = new();
}

public class GeneralIClusterLifecycleObserver(Func<Task> onStart, Func<Task> onStop) : ILifecycleObserver
{
    public Task OnStart(CancellationToken cancellationToken = default)
    {
        return onStart();
    }

    public Task OnStop(CancellationToken cancellationToken = default)
    {
        return onStop();
    }
}

public class IClusterClientLifecycleParticipant(IClusterClientLifecycleTaskCompletionSources _orleansReadySources) : ILifecycleParticipant<IClusterClientLifecycle>
{
    public void Participate(IClusterClientLifecycle lifecycle)
    {
        lifecycle.Subscribe("EntryOrleansObserver", ServiceLifecycleStage.Active, new GeneralIClusterLifecycleObserver(() => { _orleansReadySources.Actived.SetResult(); return Task.CompletedTask; }, () => Task.CompletedTask));
    }
}
