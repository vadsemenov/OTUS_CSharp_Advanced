using System.Buffers;
using System.Net.Sockets;
using System.Text;
using HighLoadedCache.Services.Abstraction;
using HighLoadedCache.Services.Utils;

namespace HighLoadedCache.Infrastructure;

public class ClientHandler(Socket clientSocket) : IClientHandler
{
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        try
        {
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
                    break;
                }

                var receivedText = Encoding.UTF8.GetString(buffer, 0, byteCount);

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