using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParametersDataEdit.CliMenuCommands;
using AppCliTools.CliParametersDataEdit.Cruders;
using Microsoft.Extensions.Logging;
using Moq;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersDataEdit.Tests.CliMenuCommands;

[Collection(ConsoleCaptureCollection.Name)]
public sealed class ConnectionStringCliMenuCommandsTests : IDisposable
{
    private const string ConnectionName = "TestConnection";
    private const string MissingName = "Missing";
    private const string GenerateCommandName = "Generate Connection String";
    private const string SetCommandName = "Set Parameters From Connection String";

    private readonly StringWriter _consoleOutput = new(CultureInfo.InvariantCulture);
    private readonly TextWriter _originalConsoleOutput;
    private readonly TestDatabaseServerParameters _parameters = new();
    private readonly Mock<IParametersManager> _parametersManager = new();
    private readonly List<string> _saveMessages = [];

    public ConnectionStringCliMenuCommandsTests()
    {
        _parameters.DatabaseServerConnections[ConnectionName] =
            DatabaseServerConnectionStringConverterTests.SqlServerData();
        _parametersManager.Setup(x => x.Parameters).Returns(_parameters);
        _parametersManager
            .Setup(x => x.Save(_parameters, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .Callback((IParameters _, string message, string? _, CancellationToken _) => _saveMessages.Add(message))
            .ReturnsAsync(true);

        _originalConsoleOutput = Console.Out;
        Console.SetOut(_consoleOutput);
    }

    private DatabaseServerConnectionData Record => _parameters.DatabaseServerConnections[ConnectionName];

    public void Dispose()
    {
        Console.SetOut(_originalConsoleOutput);
        _consoleOutput.Dispose();
    }

    [Theory]
    [InlineData(EDatabaseProvider.SqlServer, true)]
    [InlineData(EDatabaseProvider.MariaDb, true)]
    [InlineData(EDatabaseProvider.SqLite, false)]
    [InlineData(EDatabaseProvider.OleDb, false)]
    [InlineData(EDatabaseProvider.WebAgent, false)]
    public void GetItemMenu_OffersConnectionStringCommandsOnlyForServerProviders(EDatabaseProvider provider,
        bool expected)
    {
        // Arrange
        Record.DatabaseServerProvider = provider;
        var cruder = new DatabaseServerConnectionCruder(Mock.Of<IApplication>(x => x.AppName == "TestApp"),
            Mock.Of<ILogger>(), null, _parametersManager.Object, _parameters.DatabaseServerConnections);

        // Act
        List<string> names = [.. CliMenuTestAccess.GetMenuItems(cruder.GetItemMenu(ConnectionName))
            .Select(x => x.MenuItemName)];

        // Assert
        Assert.Equal(expected, names.Contains(GenerateCommandName));
        Assert.Equal(expected, names.Contains(SetCommandName));
    }

    // ---------- Generate Connection String ----------

    [Fact]
    public void Generate_ReloadsTheMenuAfterwards()
    {
        Assert.Equal(EMenuAction.Reload,
            new GenerateConnectionStringCliMenuCommand(ConnectionName, _parametersManager.Object)
                .MenuActionOnBodySuccess);
    }

    [Fact]
    public async Task Generate_WritesTheConnectionStringOfTheRecord()
    {
        // Arrange
        var sut = new GenerateConnectionStringCliMenuCommand(ConnectionName, _parametersManager.Object);

        // Act
        bool result = await CliMenuTestAccess.RunBody(sut);

        // Assert
        Assert.True(result);
        string connectionString = DatabaseServerConnectionStringConverter.CreateConnectionString(Record)!;
        Assert.StartsWith($"Connection String:{Environment.NewLine}{connectionString}{Environment.NewLine}",
            _consoleOutput.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Generate_WhenProviderCanNotCreate_Fails()
    {
        // Arrange
        Record.DatabaseServerProvider = EDatabaseProvider.SqLite;
        var sut = new GenerateConnectionStringCliMenuCommand(ConnectionName, _parametersManager.Object);

        // Act
        bool result = await CliMenuTestAccess.RunBody(sut);

        // Assert
        Assert.False(result);
        Assert.Contains("Connection String can not be created for SqLite provider", _consoleOutput.ToString(),
            StringComparison.Ordinal);
        Assert.DoesNotContain("TestPass", _consoleOutput.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Generate_WhenRecordIsMissing_Fails()
    {
        // Arrange
        var sut = new GenerateConnectionStringCliMenuCommand(MissingName, _parametersManager.Object);

        // Act
        bool result = await CliMenuTestAccess.RunBody(sut);

        // Assert
        Assert.False(result);
        Assert.Contains($"Database Server Connection {MissingName} not found", _consoleOutput.ToString(),
            StringComparison.Ordinal);
    }

    // ---------- Set Parameters From Connection String ----------

    [Fact]
    public async Task Set_WhenValid_SetsParametersAndSavesRootParameters()
    {
        // Arrange
        var sut = CreateSetCommand(ConnectionName,
            "Data Source=NewServer;Initial Catalog=AppDb;User ID=NewUser;Password=NewPass;Encrypt=False");

        // Act
        bool result = await CliMenuTestAccess.RunBody(sut);

        // Assert
        Assert.True(result);
        Assert.Equal("NewServer", Record.ServerAddress);
        Assert.Equal("NewUser", Record.ServerUser);
        Assert.Equal("NewPass", Record.ServerPass);
        Assert.False(Record.Encrypt);
        Assert.Equal([$"Database Server Connection {ConnectionName} parameters set from Connection String"],
            _saveMessages);
        Assert.Contains("Database name AppDb is not stored in Database Server Connection",
            _consoleOutput.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Set_WhenStringHasNoDatabase_DoesNotWarn()
    {
        // Arrange
        var sut = CreateSetCommand(ConnectionName, "Data Source=NewServer;Integrated Security=True");

        // Act
        bool result = await CliMenuTestAccess.RunBody(sut);

        // Assert
        Assert.True(result);
        Assert.Empty(_consoleOutput.ToString());
    }

    [Fact]
    public async Task Set_WhenSaveFails_Fails()
    {
        // Arrange
        _parametersManager
            .Setup(x => x.Save(_parameters, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var sut = CreateSetCommand(ConnectionName, "Data Source=NewServer;Integrated Security=True");

        // Act & Assert
        Assert.False(await CliMenuTestAccess.RunBody(sut));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public async Task Set_WhenInputIsEmpty_ChangesAndSavesNothing(string? input)
    {
        // Arrange
        var sut = CreateSetCommand(ConnectionName, input);

        // Act
        bool result = await CliMenuTestAccess.RunBody(sut);

        // Assert
        Assert.False(result);
        Assert.Contains("Connection String is empty", _consoleOutput.ToString(), StringComparison.Ordinal);
        Assert.Equal("TestSqlServer,1444", Record.ServerAddress);
        Assert.Empty(_saveMessages);
    }

    [Fact]
    public async Task Set_WhenInputIsInvalid_PrintsTheErrorAndSavesNothing()
    {
        // Arrange
        var sut = CreateSetCommand(ConnectionName, "User ID=NewUser");

        // Act
        bool result = await CliMenuTestAccess.RunBody(sut);

        // Assert
        Assert.False(result);
        Assert.Contains("Connection string does not contain Data Source", _consoleOutput.ToString(),
            StringComparison.Ordinal);
        Assert.Equal("TestUser", Record.ServerUser);
        Assert.Empty(_saveMessages);
    }

    [Fact]
    public async Task Set_WhenRecordIsMissing_DoesNotAskForInput()
    {
        // Arrange
        var asked = false;
        var sut = new SetParametersFromConnectionStringCliMenuCommand(MissingName, _parametersManager.Object, () =>
        {
            asked = true;
            return "Data Source=x";
        });

        // Act
        bool result = await CliMenuTestAccess.RunBody(sut);

        // Assert
        Assert.False(result);
        Assert.False(asked);
        Assert.Contains($"Database Server Connection {MissingName} not found", _consoleOutput.ToString(),
            StringComparison.Ordinal);
        Assert.Empty(_saveMessages);
    }

    //the public constructor asks on the console; the test host has no console, so the prompt is all it gets to do
    [Fact]
    public async Task Set_PublicConstructor_AsksForTheConnectionStringOnTheConsole()
    {
        // Arrange
        var sut = new SetParametersFromConnectionStringCliMenuCommand(ConnectionName, _parametersManager.Object);

        // Act
        await sut.Run();

        // Assert
        Assert.Contains("Enter Connection String", _consoleOutput.ToString(), StringComparison.Ordinal);
        Assert.Empty(_saveMessages);
    }

    private SetParametersFromConnectionStringCliMenuCommand CreateSetCommand(string name, string? input)
    {
        return new SetParametersFromConnectionStringCliMenuCommand(name, _parametersManager.Object, () => input);
    }
}
