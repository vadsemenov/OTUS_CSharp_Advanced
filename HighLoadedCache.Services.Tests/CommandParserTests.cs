using HighLoadedCache.Services;

namespace HighLoadedCache.App.Tests;

public class CommandParserTests
{
    [Fact]
    public void Parse_SetCommand_WithThreeArgs()
    {
        var input = "SET user:1 data";
        var result = CommandParser.Parse(input);

        Assert.Equal("SET", result.Command.ToString());
        Assert.Equal("user:1", result.Key.ToString());
        Assert.Equal("data", result.Value.ToString());
    }

    [Fact]
    public void Parse_GetCommand_WithTwoArgs()
    {
        var input = "GET user:1";
        var result = CommandParser.Parse(input);

        Assert.Equal("GET", result.Command.ToString());
        Assert.Equal("user:1", result.Key.ToString());
        Assert.True(result.Value.IsEmpty);
    }

    [Fact]
    public void Parse_InvalidCommand_NoKey()
    {
        var input = "SET   ";
        var result = CommandParser.Parse(input);

        Assert.True(result.Command.IsEmpty);
        Assert.True(result.Key.IsEmpty);
        Assert.True(result.Value.IsEmpty);
    }

    [Fact]
    public void Parse_Command_WithExtraSpaces()
    {
        var input = "  SET   user:1     data   ";
        var result = CommandParser.Parse(input);

        Assert.Equal("SET", result.Command.ToString());
        Assert.Equal("user:1", result.Key.ToString());
        Assert.Equal("data", result.Value.ToString());
    }
}