using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParametersDataEdit.FieldEditors;
using DatabaseTools.DbTools;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersDataEdit.Tests.ParametersEditors;

//the menu of DatabaseParametersEditor as a project or server record opens it: the sub menu of
//DatabaseParametersFieldEditor
[Collection(ConsoleCaptureCollection.Name)]
public sealed class DatabaseParametersEditorTests : IDisposable
{
    private const string PropertyName = nameof(TestRecord.DevDatabaseParameters);
    private const string OleDbConnectionName = "OldAccessDb";

    private readonly StringWriter _consoleOutput = new(CultureInfo.InvariantCulture);
    private readonly TextWriter _originalConsoleOutput;
    private readonly TestDatabaseServerParameters _parameters = new();
    private readonly Mock<IParametersManager> _parametersManager = new();
    private readonly List<string> _saveMessages = [];
    private readonly DatabaseParametersFieldEditor _sut;

    public DatabaseParametersEditorTests()
    {
        _parametersManager.Setup(x => x.Parameters).Returns(_parameters);
        _parametersManager
            .Setup(x => x.Save(_parameters, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .Callback((IParameters _, string message, string? _, CancellationToken _) => _saveMessages.Add(message))
            .ReturnsAsync(true);
        _sut = new DatabaseParametersFieldEditor(Mock.Of<IApplication>(x => x.AppName == "TestApp"),
            NullLogger.Instance, Mock.Of<IHttpClientFactory>(), PropertyName, _parametersManager.Object);

        _originalConsoleOutput = Console.Out;
        Console.SetOut(_consoleOutput);
    }

    public void Dispose()
    {
        Console.SetOut(_originalConsoleOutput);
        _consoleOutput.Dispose();
    }

    [Fact]
    public void GetSubMenu_NamesTheMenuAfterTheEditedProperty()
    {
        // Act
        CliMenuSet menu = _sut.GetSubMenu(new TestRecord());

        // Assert
        Assert.Equal($"{PropertyName} DatabaseParametersEditor", menu.Caption);
    }

    [Fact]
    public void GetSubMenu_ListsTheFieldsInOrderFollowedByDeleteAndTheEscapeCommand()
    {
        // Act
        List<string> names =
            [.. CliMenuTestAccess.GetMenuItems(_sut.GetSubMenu(new TestRecord())).Select(x => x.MenuItemName)];

        // Assert
        string[] expected =
        [
            "Edit Parameters in sequence", "Db Connection Name", "Db Server Folders Set Name",
            "Database Recovery Model", "Database Name", "Smart Schema Name", "File Storage Name", "Command Time Out",
            "Skip Backup Before Restore", "Backup Name Prefix", "Date Mask", "Backup File Extension",
            "Backup Name Middle Part", "Compress", "Verify", "Backup Type", "Delete", "Exit to level up menu"
        ];
        Assert.Equal(expected, names);
    }

    //an empty field with a default shows the default, which the tools use when the field is empty
    [Theory]
    [InlineData("Database Recovery Model", "Full")]
    [InlineData("Date Mask", "yyyyMMddHHmmss")]
    [InlineData("Backup File Extension", ".bak")]
    [InlineData("Backup Name Middle Part", "_FullDb_")]
    [InlineData("Compress", "True")]
    [InlineData("Verify", "True")]
    [InlineData("Backup Type", "Full")]
    public void GetSubMenu_WhenAFieldWithDefaultIsEmpty_ShowsTheDefault(string fieldName, string expectedStatus)
    {
        // Act
        string? status = GetStatus(new TestRecord(), fieldName);

        // Assert
        Assert.Equal(expectedStatus, status);
    }

    [Fact]
    public void GetSubMenu_WhenBackupNamePrefixIsEmpty_ShowsTheComputerName()
    {
        // Act
        string? status = GetStatus(new TestRecord(), "Backup Name Prefix");

        // Assert
        Assert.Equal($"{Environment.MachineName}_", status);
    }

    //an OleDb connection gives no database list, yet a new database name can still be entered: the editor goes on to
    //the selection, whose prompt is printed before the test host's console fails
    [Fact]
    public async Task DatabaseName_WhenTheConnectionGivesNoDatabaseList_StillOffersANewName()
    {
        // Arrange
        _parameters.DatabaseServerConnections[OleDbConnectionName] =
            new DatabaseServerConnectionData { DatabaseServerProvider = EDatabaseProvider.OleDb };
        var record = new TestRecord
        {
            DevDatabaseParameters = new DatabaseParameters
            {
                DbConnectionName = OleDbConnectionName, DatabaseName = "AppDb"
            }
        };
        CliMenuCommand databaseName = GetMenuCommand(record, "Database Name");

        // Act
        await Assert.ThrowsAnyAsync<Exception>(async () => await CliMenuTestAccess.RunBody(databaseName));

        // Assert
        Assert.Contains("Select Database Name [AppDb]: ", _consoleOutput.ToString(), StringComparison.Ordinal);
        Assert.Empty(_saveMessages);
    }

    //the whole record goes (null): tools count even an empty object as given parameters
    [Fact]
    public async Task Delete_DeletesTheEditedRecordAndSavesItsParent()
    {
        // Arrange
        var record = new TestRecord
        {
            DevDatabaseParameters = new DatabaseParameters { DbConnectionName = "DevSqlServer", DatabaseName = "AppDb" }
        };
        CliMenuCommand deleteCommand = GetMenuCommand(record, "Delete");

        // Act
        bool result = await CliMenuTestAccess.RunBody(deleteCommand);

        // Assert
        Assert.True(result);
        Assert.Null(record.DevDatabaseParameters);
        Assert.Equal("(empty)", _sut.GetValueStatus(record));
        Assert.Equal($"{PropertyName} deleted", Assert.Single(_saveMessages));
    }

    private CliMenuCommand GetMenuCommand(TestRecord record, string name)
    {
        return CliMenuTestAccess.GetMenuItems(_sut.GetSubMenu(record)).Single(x => x.MenuItemName == name)
            .CliMenuCommand;
    }

    private string? GetStatus(TestRecord record, string fieldName)
    {
        CliMenuCommand command = GetMenuCommand(record, fieldName);
        command.CountStatus();
        return command.StatusString;
    }

    //as ProjectModel and ServerInfoModel keep it: an init-only property that the field editor sets by reflection
    private sealed class TestRecord
    {
        public DatabaseParameters? DevDatabaseParameters { get; init; }
    }
}
