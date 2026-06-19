using Interfaces;
using Orleans;

namespace ServerCommon.Services.Test;

public interface ITestService
{
    Task<string> GetTestAsync();
}

public class TestService(
    IClusterClient clusterClient
) : ITestService
{
    readonly IPing pingGrain = clusterClient.GetGrain<IPing>(0);
    public async Task<string> GetTestAsync()
    {
        return await pingGrain.Ping();
    }
}