namespace Interfaces;

[Alias("Interfaces.IPing")]
public interface IPing : IGrainWithIntegerKey
{
    [Alias("Ping")]
    Task<string> Ping();
}
