using Interfaces;

namespace Grains;

public class Hello : Grain, IPing
{
    public Task<string> Ping()
    {
        return Task.FromResult("Pong!");
    }
}