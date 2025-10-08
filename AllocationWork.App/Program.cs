namespace AllocationWork.App;

public class Program
{
    static async Task Main(string[] args)
    {
        await using var server = new TcpServer();

        try
        {
            using var cancellationTokenSource = new CancellationTokenSource();

            // Запуск сервера в фоновом режиме
            await server.StartAsync(cancellationTokenSource.Token);

            Console.WriteLine("Сервер запущен. Нажмите Enter для остановки...");
            Console.ReadLine();

            await cancellationTokenSource.CancelAsync();

            Console.WriteLine("Остановка сервера...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при работе сервера: {ex.Message}");
        }
    }
}