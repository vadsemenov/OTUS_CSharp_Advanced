namespace HighLoadedCache.Services.Abstraction;

public interface IClientHandler
{
    Task ProcessAsync(CancellationToken cancellationToken);
}