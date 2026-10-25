using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using AppCliTools.CliParameters.FieldEditors;
using SystemTools.SystemToolsShared;
using Xunit;

namespace AppCliTools.CliParameters.Tests.FieldEditors;

[Collection(ConsoleCaptureCollection.Name)]
public sealed class IntFieldEditorTests : IDisposable
{
    private readonly StringWriter _consoleOutput = new(CultureInfo.InvariantCulture);
    private readonly TextWriter _originalConsoleOutput;

    public IntFieldEditorTests()
    {
        _originalConsoleOutput = Console.Out;
        Console.SetOut(_consoleOutput);
    }

    public void Dispose()
    {
        Console.SetOut(_originalConsoleOutput);
        _consoleOutput.Dispose();
    }

    //the test host has no console input, so reading the number fails right after the prompt is printed: the prompt
    //offers the value of the record
    [Fact]
    public async Task UpdateField_OnTheConsole_OffersTheValueOfTheRecord()
    {
        // Arrange
        var record = new TestRecord { Count = 5 };
        var sut = new IntFieldEditor(nameof(TestRecord.Count), 3);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.UpdateField(null, record));

        // Assert
        Assert.Contains("Count [5]: ", _consoleOutput.ToString(), StringComparison.Ordinal);
        Assert.Equal(5, record.Count);
    }

    //a new record of a cruder gets the default of every field editor, so the given default must reach the record
    //instead of 0
    [Fact]
    public void SetDefault_WithDefaultValue_SetsTheDefaultValue()
    {
        // Arrange
        var record = new TestRecord { Count = 7 };
        var sut = new IntFieldEditor(nameof(TestRecord.Count), 3);

        // Act
        sut.SetDefault(record);

        // Assert
        Assert.Equal(3, record.Count);
    }

    [Fact]
    public void SetDefault_WithoutDefaultValue_SetsZero()
    {
        // Arrange
        var record = new TestRecord { Count = 7 };
        var sut = new IntFieldEditor(nameof(TestRecord.Count));

        // Act
        sut.SetDefault(record);

        // Assert
        Assert.Equal(0, record.Count);
    }

    //the default is only for new records: the status of a record still shows its own value
    [Fact]
    public void GetValueStatus_WithDefaultValue_ShowsTheValueOfTheRecord()
    {
        // Arrange
        var record = new TestRecord { Count = 5 };
        var sut = new IntFieldEditor(nameof(TestRecord.Count), 3);

        // Act
        string result = sut.GetValueStatus(record);

        // Assert
        Assert.Equal("5", result);
    }

    private sealed class TestRecord : ItemData
    {
        public int Count { get; set; }
    }
}
