using System.Net.Sockets;
using System.Text;

try
{
    using var client = new TcpClient();
    await client.ConnectAsync("127.0.0.1", 8081);

    await using var stream = client.GetStream();
    var commands = new[]
    {
        "SET key1 value1",
        "GET key1",
        "DELETE key1",
        "PING"
    };

    while (Console.ReadKey().Key != ConsoleKey.Escape)
    {
        foreach (var command in commands)
        {
            var data = Encoding.UTF8.GetBytes(command);
            await stream.WriteAsync(data);
            Console.WriteLine($"Отправлено: {command}");

            Console.ReadKey();
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка клиента: {ex.Message}");
}