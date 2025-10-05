namespace AllocationWork.App.Tests;

public class SimpleStoreConcurrencyTests
{
    [Fact]
    public async Task Parallel_Set_and_Get_are_thread_safe_and_counters_correct()
    {
        const int keyCount = 200;
        const int writers = 16;
        const int writesPerWriter = 1000; // итого ожидаемых Set: writers * writesPerWriter
        const int readers = 32;
        const int readsPerReader = 1500;  // итого ожидаемых Get: readers * readsPerReader

        using var store = new SimpleStore();

        // Подготовка ключей и начальная запись
        var keys = Enumerable.Range(0, keyCount).Select(i => $"key-{i}").ToArray();
        foreach (var k in keys)
            store.Set(k, BitConverter.GetBytes(0));

        // Барьер, чтобы все потоки стартовали максимально синхронно
        var start = new TaskCompletionSource();

        var rnd = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        // Писатели: перезаписывают случайные ключи новыми значениями-счетчиками
        var writerTasks = Enumerable.Range(0, writers).Select(wi => Task.Run(async () =>
        {
            await start.Task;
            for (int i = 1; i <= writesPerWriter; i++)
            {
                var k = keys[rnd.Value!.Next(keys.Length)];
                // Записываем значение как [writerIndex, i] для проверки последствий гонок
                var payload = BitConverter.GetBytes((wi << 16) | i);
                store.Set(k, payload);
                // Малая вариативная задержка для «мешалки»
                if ((i & 0xFF) == 0) await Task.Yield();
            }
        })).ToArray();

        // Читатели: читают случайные ключи и валидируют, что чтение не падает
        var readerTasks = Enumerable.Range(0, readers).Select(ri => Task.Run(async () =>
        {
            await start.Task;
            int localNulls = 0;
            for (int i = 0; i < readsPerReader; i++)
            {
                var k = keys[rnd.Value!.Next(keys.Length)];
                var val = store.Get(k);
                if (val is null) Interlocked.Increment(ref localNulls);
                if ((i & 0x3FF) == 0) await Task.Yield();
            }
            // Не ожидаем null, так как ключи предварительно добавлены
            Assert.Equal(0, localNulls);
        })).ToArray();

        // Запуск
        start.SetResult();
        await Task.WhenAll(writerTasks.Concat(readerTasks));

        // Проверки счетчиков
        var (setCount, getCount, deleteCount) = store.GetStatistics();
        Assert.Equal(writers * writesPerWriter + keyCount, setCount); // включая прогревочные Set
        Assert.Equal(readers * readsPerReader, getCount);
        Assert.Equal(0, deleteCount);

        // Проверка целостности: каждый ключ присутствует и значение имеет ожидаемую длину
        foreach (var k in keys)
        {
            var v = store.Get(k);
            Assert.NotNull(v);
            Assert.True(v!.Length == sizeof(int)); // записывали 4 байта
        }
    }
}