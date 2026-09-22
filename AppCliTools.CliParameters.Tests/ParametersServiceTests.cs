using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParameters.Tests;

//ParametersService: პარამეტრების ფაილის დადგენა, ჩატვირთვა და საჭიროების შემთხვევაში შექმნა
[Collection(ConsoleCaptureCollection.Name)]
public sealed class ParametersServiceTests : IDisposable
{
    private const string AppName = "TestApp";

    private readonly StringWriter _consoleOutput = new(CultureInfo.InvariantCulture);
    private readonly TextWriter _originalConsoleOutput;
    private readonly List<bool> _defaultAnswers = [];
    private readonly string _originalCurrentDirectory;
    private readonly List<string> _questions = [];
    private readonly string _rootFolder;

    public ParametersServiceTests()
    {
        _rootFolder = Directory.CreateTempSubdirectory("AppCliToolsTests_").FullName;
        _originalCurrentDirectory = Directory.GetCurrentDirectory();
        DeleteExecutableFolderParametersFile();
        _originalConsoleOutput = Console.Out;
        Console.SetOut(_consoleOutput);
    }

    public void Dispose()
    {
        Console.SetOut(_originalConsoleOutput);
        _consoleOutput.Dispose();
        Directory.SetCurrentDirectory(_originalCurrentDirectory);
        DeleteExecutableFolderParametersFile();
        Directory.Delete(_rootFolder, true);
    }

    [Fact]
    public void Analysis_WhenFileNameIsNotSpecifiedAndNothingIsFound_AsksForHelp()
    {
        // Arrange
        Directory.SetCurrentDirectory(_rootFolder);
        ParametersService<TestParameters> sut = CreateSut(false);

        // Act
        EParseResult result = sut.Analysis(null);

        // Assert
        Assert.Equal(EParseResult.ShowHelp, result);
        Assert.Null(sut.Par);
        //ავტომატური ძებნისას ფაილის შექმნა არ უნდა შემოთავაზდეს
        Assert.Empty(_questions);
    }

    [Fact]
    public void Analysis_WhenFileNameIsNotSpecifiedAndCurrentFolderHasTheFile_LoadsItWithoutAnyQuestion()
    {
        // Arrange
        WriteParametersFile("FromCurrentFolder", $"{AppName}.json");
        Directory.SetCurrentDirectory(_rootFolder);
        ParametersService<TestParameters> sut = CreateSut(false);

        // Act
        EParseResult result = sut.Analysis(null);

        // Assert
        Assert.Equal(EParseResult.Ok, result);
        Assert.Equal("FromCurrentFolder", sut.Par?.Name);
        Assert.Equal(Path.Combine(_rootFolder, $"{AppName}.json"), sut.ParametersFileName);
        Assert.Empty(_questions);
    }

    [Fact]
    public void Analysis_WhenOnlyExecutableFolderHasTheFile_LoadsItFromThere()
    {
        // Arrange
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, $"{AppName}.json"),
            JsonConvert.SerializeObject(new TestParameters { Name = "FromExecutableFolder" }));
        Directory.SetCurrentDirectory(_rootFolder);
        ParametersService<TestParameters> sut = CreateSut(false);

        // Act
        EParseResult result = sut.Analysis(null);

        // Assert
        Assert.Equal(EParseResult.Ok, result);
        Assert.Equal("FromExecutableFolder", sut.Par?.Name);
        Assert.Empty(_questions);
    }

    [Fact]
    public void Analysis_WhenFileIsValid_LoadsParameters()
    {
        // Arrange
        string fileName = WriteParametersFile("MyName");
        ParametersService<TestParameters> sut = CreateSut(false);

        // Act
        EParseResult result = sut.Analysis(fileName);

        // Assert
        Assert.Equal(EParseResult.Ok, result);
        Assert.Equal(fileName, sut.ParametersFileName);
        Assert.NotNull(sut.Par);
        Assert.Equal("MyName", sut.Par.Name);
        Assert.Empty(_questions);
    }

    [Fact]
    public void Analysis_WhenFileIsNotValidAndRewriteIsRefused_ReturnsParseError()
    {
        // Arrange
        string fileName = Path.Combine(_rootFolder, "broken.json");
        File.WriteAllText(fileName, "this is not json");
        ParametersService<TestParameters> sut = CreateSut(false);

        // Act
        EParseResult result = sut.Analysis(fileName);

        // Assert
        Assert.Equal(EParseResult.ParseError, result);
        Assert.Null(sut.Par);
        Assert.Equal(fileName, sut.ParametersFileName);
        Assert.Equal("this is not json", File.ReadAllText(fileName));
        Assert.Contains($"File {fileName} is not valid parameters file", ConsoleText(), StringComparison.Ordinal);
        Assert.Contains($"File {fileName} is Invalid, Create, rewrite and use file with this name?", _questions,
            StringComparer.Ordinal);
    }

    [Fact]
    public void Analysis_WhenFileIsNotValidAndRewriteIsAllowed_RewritesFileWithEmptyParameters()
    {
        // Arrange
        string fileName = Path.Combine(_rootFolder, "broken.json");
        File.WriteAllText(fileName, "this is not json");
        ParametersService<TestParameters> sut = CreateSut(true);

        // Act
        EParseResult result = sut.Analysis(fileName);

        // Assert
        Assert.Equal(EParseResult.ParseError, result);
        Assert.Equal("{}", File.ReadAllText(fileName));
        Assert.Contains($"New parameters saved to file {fileName}", ConsoleText(), StringComparison.Ordinal);
    }

    [Fact]
    public void Analysis_WhenFileDoesNotExistAndCreationIsRefused_ReturnsParseError()
    {
        // Arrange
        string fileName = Path.Combine(_rootFolder, "absent.json");
        ParametersService<TestParameters> sut = CreateSut(false);

        // Act
        EParseResult result = sut.Analysis(fileName);

        // Assert
        Assert.Equal(EParseResult.ParseError, result);
        Assert.False(File.Exists(fileName));
        Assert.Contains($"File {fileName} is not exists", ConsoleText(), StringComparison.Ordinal);
        Assert.Contains($"File {fileName} is not exists, Create and use file with this name?", _questions,
            StringComparer.Ordinal);
    }

    [Fact]
    public void Analysis_WhenFileDoesNotExistAndCreationIsAllowed_CreatesEmptyParametersFile()
    {
        // Arrange
        string fileName = Path.Combine(_rootFolder, "absent.json");
        ParametersService<TestParameters> sut = CreateSut(true);

        // Act
        EParseResult result = sut.Analysis(fileName);

        // Assert
        Assert.Equal(EParseResult.ParseError, result);
        Assert.True(File.Exists(fileName));
        Assert.Equal("{}", File.ReadAllText(fileName));
    }

    [Fact]
    public void Analysis_WhenFolderOfTheFileDoesNotExist_CreatesFolderAndFile()
    {
        // Arrange
        string fileName = Path.Combine(_rootFolder, "newFolder", "absent.json");
        ParametersService<TestParameters> sut = CreateSut(true);

        // Act
        EParseResult result = sut.Analysis(fileName);

        // Assert
        Assert.Equal(EParseResult.ParseError, result);
        Assert.True(File.Exists(fileName));
    }

    [Fact]
    public void Analysis_WhenFileNameIsARootFolder_ReturnsParseError()
    {
        // Arrange
        string rootName = Path.GetPathRoot(_rootFolder)!;
        ParametersService<TestParameters> sut = CreateSut(true);

        // Act
        EParseResult result = sut.Analysis(rootName);

        // Assert
        Assert.Equal(EParseResult.ParseError, result);
        Assert.Contains($"Invalid file name {rootName} for Parameters", ConsoleText(), StringComparison.Ordinal);
        Assert.Empty(_questions);
    }

    [Fact]
    public void Analysis_WhenCalledSecondTimeWithValidFile_ReplacesPreviouslyLoadedParameters()
    {
        // Arrange
        string firstFileName = WriteParametersFile("First", "first.json");
        string secondFileName = WriteParametersFile("Second", "second.json");
        ParametersService<TestParameters> sut = CreateSut(false);

        // Act
        EParseResult firstResult = sut.Analysis(firstFileName);
        EParseResult secondResult = sut.Analysis(secondFileName);

        // Assert
        Assert.Equal(EParseResult.Ok, firstResult);
        Assert.Equal(EParseResult.Ok, secondResult);
        Assert.Equal(secondFileName, sut.ParametersFileName);
        Assert.Equal("Second", sut.Par?.Name);
    }
    [Fact]
    public void AnalyzeParamFileName_WhenNameIsNotSpecifiedAndCurrentFolderHasTheFile_LoadsIt()
    {
        // Arrange
        WriteParametersFile("FromCurrentFolder", $"{AppName}.json");
        Directory.SetCurrentDirectory(_rootFolder);
        ParametersService<TestParameters> sut = CreateSut(false);

        // Act
        bool result = sut.AnalyzeParamFileName(null);

        // Assert
        Assert.True(result);
        Assert.Equal("FromCurrentFolder", sut.Par?.Name);
        Assert.Contains("file name is not specified", ConsoleText(), StringComparison.Ordinal);
        Assert.Contains($"Try to use current Directory {_rootFolder}", ConsoleText(), StringComparison.Ordinal);
    }

    [Fact]
    public void AnalyzeParamFileName_WhenNameIsNotSpecifiedAndNothingIsFound_ReportsBothFolders()
    {
        // Arrange
        Directory.SetCurrentDirectory(_rootFolder);
        ParametersService<TestParameters> sut = CreateSut(true);

        // Act
        bool result = sut.AnalyzeParamFileName(null);

        // Assert
        Assert.False(result);
        Assert.Null(sut.Par);
        Assert.Contains("Try to use current Directory" + Environment.NewLine, ConsoleText(), StringComparison.Ordinal);
        //ორივე ფოლდერზე არარსებობა გამოცხადდა, მაგრამ შექმნა არ შემოთავაზებულა
        Assert.Contains($"File {Path.Combine(_rootFolder, $"{AppName}.json")} is not exists", ConsoleText(),
            StringComparison.Ordinal);
        Assert.Contains($"File {Path.Combine(AppContext.BaseDirectory, $"{AppName}.json")} is not exists",
            ConsoleText(), StringComparison.Ordinal);
        Assert.Empty(_questions);
    }

    [Fact]
    public void TryUseFile_WhenFileIsCreatedOnDemand_ReturnsTrue()
    {
        // Arrange
        string fileName = Path.Combine(_rootFolder, "created.json");
        ParametersService<TestParameters> sut = CreateSut(true);

        // Act
        bool result = sut.TryUseFile(fileName);

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(fileName));
        Assert.Equal(fileName, sut.ParametersFileName);
        Assert.Equal([true], _defaultAnswers);
    }

    [Fact]
    public void TryUseFile_WhenBrokenFileRewriteIsOffered_DefaultAnswerIsNo()
    {
        // Arrange
        string fileName = Path.Combine(_rootFolder, "broken.json");
        File.WriteAllText(fileName, "not a json");
        ParametersService<TestParameters> sut = CreateSut(true);

        // Act
        bool result = sut.TryUseFile(fileName);

        // Assert
        Assert.True(result);
        Assert.Equal([false], _defaultAnswers);
    }

    [Fact]
    public void TryUseFile_WhenFileNameIsARootFolder_ReturnsFalse()
    {
        // Arrange
        ParametersService<TestParameters> sut = CreateSut(true);

        // Act
        bool result = sut.TryUseFile(Path.GetPathRoot(_rootFolder)!);

        // Assert
        Assert.False(result);
        Assert.Empty(_questions);
    }

    [Fact]
    public void CreateEmptyParametersFile_WritesEmptyJsonAndReturnsTrue()
    {
        // Arrange
        string fileName = Path.Combine(_rootFolder, "empty.json");

        // Act
        bool result = ParametersService<TestParameters>.CreateEmptyParametersFile(fileName);

        // Assert
        Assert.True(result);
        Assert.Equal("{}", File.ReadAllText(fileName));
        Assert.Contains($"New parameters saved to file {fileName}", ConsoleText(), StringComparison.Ordinal);
    }

    //გამშვები ფაილის ფოლდერში დარჩენილი ფაილი მომდევნო ტესტის შედეგს შეცვლიდა
    private static void DeleteExecutableFolderParametersFile()
    {
        string fileName = Path.Combine(AppContext.BaseDirectory, $"{AppName}.json");
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }
    }
    [Fact]
    public void Analysis_WhenNextFileIsMissingAndCreationIsRefused_ReturnsParseErrorAlthoughOldParametersAreStillThere()
    {
        // Arrange
        string loadableFileName = WriteParametersFile("First", "first.json");
        string absentFileName = Path.Combine(_rootFolder, "absent.json");
        ParametersService<TestParameters> sut = CreateSut(false);

        // Act
        EParseResult firstResult = sut.Analysis(loadableFileName);
        EParseResult secondResult = sut.Analysis(absentFileName);

        // Assert
        Assert.Equal(EParseResult.Ok, firstResult);
        Assert.Equal(EParseResult.ParseError, secondResult);
        Assert.Equal("First", sut.Par?.Name);
    }

    [Fact]
    public void Analysis_WhenFileNameIsNotSpecifiedAfterSuccessfulLoad_DoesNotReuseAlreadyLoadedParameters()
    {
        // Arrange
        string loadableFileName = WriteParametersFile("First", "first.json");
        ParametersService<TestParameters> sut = CreateSut(false);
        Directory.SetCurrentDirectory(_rootFolder);

        // Act
        EParseResult firstResult = sut.Analysis(loadableFileName);
        EParseResult secondResult = sut.Analysis(null);

        // Assert
        Assert.Equal(EParseResult.Ok, firstResult);
        //მეორედ ფაილი ვერ მოიძებნა, ამიტომ ძველი პარამეტრები აღარ ჩაითვლება ნაპოვნად
        Assert.Equal(EParseResult.ShowHelp, secondResult);
    }

    [Fact]
    public void TryUseFile_WhenBrokenFileMayNotBeRewritten_ReturnsFalseWithoutAnyQuestion()
    {
        // Arrange
        string fileName = Path.Combine(_rootFolder, "broken.json");
        File.WriteAllText(fileName, "not a json");
        ParametersService<TestParameters> sut = CreateSut(true);

        // Act
        bool result = sut.TryUseFile(fileName, false);

        // Assert
        Assert.False(result);
        Assert.Empty(_questions);
        Assert.Equal("not a json", File.ReadAllText(fileName));
    }

    private string ConsoleText()

    {
        return _consoleOutput.ToString();
    }

    private string WriteParametersFile(string name, string fileName = "par.json")
    {
        string fullName = Path.Combine(_rootFolder, fileName);
        File.WriteAllText(fullName, JsonConvert.SerializeObject(new TestParameters { Name = name }));
        return fullName;
    }

    //ტესტები კონსოლიდან კითხვის ნაცვლად თვითონ აბრუნებენ პასუხს და კითხვების ტექსტს იმახსოვრებენ
    private ParametersService<TestParameters> CreateSut(bool answer)
    {
        return new ParametersService<TestParameters>(AppName, (question, defaultAnswer) =>
        {
            _questions.Add(question);
            _defaultAnswers.Add(defaultAnswer);
            return answer;
        });
    }

    private sealed class TestParameters : IParameters
    {
        public string? Name { get; set; }

        public bool CheckBeforeSave()
        {
            return true;
        }
    }
}
