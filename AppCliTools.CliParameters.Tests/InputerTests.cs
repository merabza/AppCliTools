using System;
using System.Globalization;
using System.IO;
using AppCliTools.LibDataInput;

namespace AppCliTools.CliParameters.Tests;

//Inputer.AnswerYesToAll (SupportTools-ის --allyes): y/n შეკითხვები კონსოლს აღარ კითხულობენ. ტესტის ჰოსტის შეყვანა
//გადამისამართებულია და Console.ReadKey გამონაკლისს ისვრის, ამიტომ კონსოლის წაკითხვა ტესტში ყოველთვის გამოჩნდება.
//გადამრთველი მთელ პროცესზე მოქმედებს, ამიტომ კლასი სხვა ტესტების პარალელურად არ უნდა გაეშვას
[Collection(ConsoleCaptureCollection.Name)]
public sealed class InputerTests : IDisposable
{
    private readonly StringWriter _consoleOutput = new(CultureInfo.InvariantCulture);
    private readonly TextWriter _originalConsoleOutput;

    public InputerTests()
    {
        _originalConsoleOutput = Console.Out;
        Console.SetOut(_consoleOutput);
    }

    public void Dispose()
    {
        Inputer.AnswerYesToAll = false;
        Console.SetOut(_originalConsoleOutput);
        _consoleOutput.Dispose();
    }

    [Theory]
    [InlineData(true, "y")]
    [InlineData(false, "n")]
    public void InputBool_WhenAnswerYesToAll_ReturnsTrueWithoutReadingTheConsole(bool defaultValue, string shownDefault)
    {
        // Arrange
        Inputer.AnswerYesToAll = true;

        // Act
        bool result = Inputer.InputBool("Overwrite?", defaultValue);

        // Assert
        Assert.True(result);
        Assert.Equal($"Overwrite? (y/n)[{shownDefault}]: Y (yes to all){Environment.NewLine}", ConsoleText());
    }

    [Fact]
    public void InputAnswer_WhenAnswerYesToAll_ReturnsYesWithoutReadingTheConsole()
    {
        // Arrange
        Inputer.AnswerYesToAll = true;

        // Act
        EDialogResult result = Inputer.InputAnswer("Delete?", EDialogResult.No);

        // Assert
        Assert.Equal(EDialogResult.Yes, result);
        Assert.Equal($"Delete?: Y (yes to all){Environment.NewLine}", ConsoleText());
    }

    //ნაგულისხმევად გადამრთველი გამორთულია და შეკითხვა კონსოლს კითხულობს
    [Fact]
    public void InputBool_ByDefault_ReadsTheConsole()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => Inputer.InputBool("Overwrite?", true));
        Assert.Equal("Overwrite? (y/n)[y]: ", ConsoleText());
    }

    private string ConsoleText()
    {
        return _consoleOutput.ToString();
    }
}
