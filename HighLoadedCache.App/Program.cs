using HighLoadedCache.Infrastructure;
using HighLoadedCache.Services.Abstraction;
using HighLoadedCache.Services.Store;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ISimpleStore, SimpleStore>();
builder.Services.AddSingleton<ITcpServer, TcpServer>();
builder.Services.AddScoped<IClientHandler, ClientHandler>();

var host = builder.Build();

await RunTcpServerAsync(host);

await host.RunAsync();

await host.WaitForShutdownAsync();

async Task RunTcpServerAsync(IHost host)
{
    await using var server = host.Services.GetService<ITcpServer>();

    try
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        // Запуск сервера в фоновом режиме
        await server!.StartAsync(cancellationTokenSource.Token);

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

//
// public class Program
// {
//     static async Task Main(string[] args)
//     {
//         await using var server = new TcpServer();
//
//         try
//         {
//             using var cancellationTokenSource = new CancellationTokenSource();
//
//             // Запуск сервера в фоновом режиме
//             await server.StartAsync(cancellationTokenSource.Token);
//
//             Console.WriteLine("Сервер запущен. Нажмите Enter для остановки...");
//             Console.ReadLine();
//
//             await cancellationTokenSource.CancelAsync();
//
//             Console.WriteLine("Остановка сервера...");
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Ошибка при работе сервера: {ex.Message}");
//         }
//     }
// }