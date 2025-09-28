namespace AllocationWork.App;

public class Program
{
    static async Task Main(string[] args)
    {
        using var server = new TcpServer();

        try
        {
            var cancellationTokenSource = new CancellationTokenSource();

            // Запуск сервера в фоновом режиме
            var serverTask = server.StartAsync(cancellationTokenSource.Token);

            // await Task.Delay(3000, cancellationTokenSource.Token);

            // await TestClient.TestAsync();

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