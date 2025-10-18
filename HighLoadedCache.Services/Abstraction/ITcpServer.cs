namespace HighLoadedCache.Services.Abstraction;

public interface ITcpServer: IAsyncDisposable
{
    Task StartAsync(CancellationToken cancellationToken);
}