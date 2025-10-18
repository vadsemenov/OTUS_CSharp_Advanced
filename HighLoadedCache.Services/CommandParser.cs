namespace HighLoadedCache.Services;

public static class CommandParser
{
    public static CommandParts Parse(ReadOnlySpan<char> input)
    {
        input = input.Trim();

        int firstSpace = input.IndexOf(' ');
        if (firstSpace == -1)
        {
            return default;
        }

        var command = input.Slice(0, firstSpace).Trim();
        var rest = input.Slice(firstSpace + 1).Trim();
        if (rest.IsEmpty)
        {
            return default;
        }

        int secondSpace = rest.IndexOf(' ');
        if (secondSpace == -1)
        {
            return new CommandParts
            {
                Command = command,
                Key = rest,
                Value = ReadOnlySpan<char>.Empty
            };
        }
        else
        {
            return new CommandParts
            {
                Command = command,
                Key = rest.Slice(0, secondSpace).Trim(),
                Value = rest.Slice(secondSpace + 1).Trim()
            };
        }
    }
}

public ref struct CommandParts
{
    public ReadOnlySpan<char> Command;
    public ReadOnlySpan<char> Key;
    public ReadOnlySpan<char> Value;
}