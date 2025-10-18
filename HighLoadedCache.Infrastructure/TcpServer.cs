using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HighLoadedCache.Services;

namespace HighLoadedCache.Infrastructure;

public class TcpServer : IAsyncDisposable
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

                    // Создаем и запускаем обработчик клиента
                    var clientHandler = new ClientHandler(clientSocket);
                    _connectedClients.Add(clientHandler);

                    Console.WriteLine(
                        $"Создано новое подключение с клиентом. Всего подключений: {_connectedClients.Count}");

                    // Запускаем обработку клиента в отдельной задаче
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await clientHandler.ProcessAsync(cancellationToken);
                        }
                        finally
                        {
                            // Удаляем клиента из списка при отключении
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

// Класс для обработки отдельного клиента
public class ClientHandler(Socket clientSocket)
{
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Отправляем приветственное сообщение
            var welcomeMessage = "Подключение к серверу установлено\n";
            var welcomeBytes = Encoding.UTF8.GetBytes(welcomeMessage);
            await clientSocket.SendAsync(welcomeBytes, SocketFlags.None);

            await ReceiveDataAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обработки клиента: {ex.Message}");
        }
    }

    private async Task ReceiveDataAsync(CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(200);

        try
        {
            while (clientSocket.Connected)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var byteCount = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);

                if (byteCount == 0)
                {
                    // Клиент отключился
                    break;
                }

                // Обрабатываем полученные данные
                var receivedText = Encoding.UTF8.GetString(buffer, 0, byteCount);

                // Выводим информацию о команде
                PrintCommandsToConsole(CommandParser.Parse(receivedText.AsSpan()));
            }
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
        {
            Console.WriteLine("Клиент разорвал соединение");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении данных: {ex.Message}");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private void PrintCommandsToConsole(CommandParts commands)
    {
        Console.WriteLine(
            $"[Клиент {clientSocket.RemoteEndPoint}] Команда {commands.Command}, ключ {commands.Key}, значение {commands.Value}");
    }

    public Task DisconnectAsync()
    {
        try
        {
            if (clientSocket.Connected)
            {
                clientSocket.Shutdown(SocketShutdown.Both);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отключении клиента: {ex.Message}");
        }
        finally
        {
            clientSocket.Close();
            clientSocket.Dispose();
        }

        return Task.CompletedTask;
    }
}