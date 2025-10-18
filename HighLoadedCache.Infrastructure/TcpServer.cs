using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using HighLoadedCache.Services.Abstraction;

namespace HighLoadedCache.Infrastructure;

public class TcpServer : ITcpServer
{
    private Socket? _socket;
    private readonly ConcurrentBag<ClientHandler> _connectedClients = new();
    private const string IpAddress = "127.0.0.1";
    private const int Port = 8081;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var localEndpoint = new IPEndPoint(IPAddress.Parse(IpAddress), Port);

            _socket.Bind(localEndpoint);
            _socket.Listen(100);

            Console.WriteLine("Сервер запущен. Ожидание подключений...");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var clientSocket = await _socket.AcceptAsync(cancellationToken);

                    var clientHandler = new ClientHandler(clientSocket);
                    _connectedClients.Add(clientHandler);

                    Console.WriteLine(
                        $"Создано новое подключение с клиентом. Всего подключений: {_connectedClients.Count}");

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await clientHandler.ProcessAsync(cancellationToken);
                        }
                        finally
                        {
                            _connectedClients.TryTake(out clientHandler);

                            Console.WriteLine($"Клиент отключен. Всего подключений: {_connectedClients.Count}");
                        }
                    }, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Ошибка при принятии подключения: {exception}");
                }
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }

    private async Task DisconnectAllClientsAsync()
    {
        var disconnectTasks = _connectedClients.Select(client => client.DisconnectAsync());
        await Task.WhenAll(disconnectTasks);

        _connectedClients.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAllClientsAsync();

        _socket?.Dispose();
    }
}