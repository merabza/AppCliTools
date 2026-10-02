using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.CliParameters.Cruders;
using AppCliTools.CliParametersEdit.Cruders;
using AppCliTools.CliParametersEdit.FieldEditors;
using Moq;
using ParametersManagement.LibFileParameters.Interfaces;
using ParametersManagement.LibFileParameters.Models;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersEdit.Tests.FieldEditors;

//დეტალების სიის cruder-ი SmartSchema-ს რედაქტორის IParametersManager-ით ინახავს
public sealed class SmartSchemaDetailsFieldEditorTests
{
    private readonly Mock<IParametersWithSmartSchemas> _parameters = new();
    private readonly Mock<IParametersManager> _parametersManager = new();

    public SmartSchemaDetailsFieldEditorTests()
    {
        _parametersManager.SetupGet(x => x.Parameters).Returns(_parameters.Object);
        _parametersManager.Setup(x => x.Save(It.IsAny<IParameters>(), It.IsAny<string>(), It.IsAny<string?>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task GetSubMenu_WhenCreatedWithParametersManager_ListsDetailsWithASavingCruder()
    {
        // Arrange
        var smartSchema = new SmartSchema { Details = { new SmartSchemaDetail { PeriodType = EPeriodType.Day } } };
        var sut = new SmartSchemaDetailsFieldEditor(nameof(SmartSchema.Details), _parametersManager.Object);

        // Act
        Cruder cruder = CliMenuTestAccess.GetListMenuCruder(sut.GetSubMenu(smartSchema));
        await cruder.Save("Saved");

        // Assert
        Assert.Equal([nameof(EPeriodType.Day)], cruder.GetKeys());
        _parametersManager.Verify(x => x.Save(_parameters.Object, "Saved", null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    //SmartSchema-ს რედაქტორი დეტალების რედაქტორს თავის IParametersManager-ს გადასცემს
    [Fact]
    public async Task SmartSchemaCruder_WhenCreated_GivesItsParametersManagerToTheDetailsEditor()
    {
        // Arrange
        var smartSchemaCruder = new SmartSchemaCruder(_parametersManager.Object, []);
        var sut = (SmartSchemaDetailsFieldEditor)CliMenuTestAccess.GetFieldEditors(smartSchemaCruder)
            .Find(x => x.PropertyName == nameof(SmartSchema.Details))!;

        // Act
        CliMenuSet detailsMenu = sut.GetSubMenu(new SmartSchema());
        await CliMenuTestAccess.GetListMenuCruder(detailsMenu).Save("Saved");

        // Assert
        _parametersManager.Verify(x => x.Save(_parameters.Object, "Saved", null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSubMenu_WhenCreatedWithoutParametersManager_ListsDetailsWithoutSaving()
    {
        // Arrange
        var smartSchema = new SmartSchema { Details = { new SmartSchemaDetail { PeriodType = EPeriodType.Week } } };
        var sut = new SmartSchemaDetailsFieldEditor(nameof(SmartSchema.Details));

        // Act
        Cruder cruder = CliMenuTestAccess.GetListMenuCruder(sut.GetSubMenu(smartSchema));
        bool result = await cruder.Save("Saved");

        // Assert
        Assert.True(result);
        Assert.Equal([nameof(EPeriodType.Week)], cruder.GetKeys());
    }

    //a schema without a details list shows an empty list instead of failing
    [Fact]
    public void GetSubMenu_WhenRecordHasNoDetailsList_ListsNoDetails()
    {
        // Arrange
        var smartSchema = new SmartSchema { Details = null! };
        var sut = new SmartSchemaDetailsFieldEditor(nameof(SmartSchema.Details), _parametersManager.Object);

        // Act
        Cruder cruder = CliMenuTestAccess.GetListMenuCruder(sut.GetSubMenu(smartSchema));

        // Assert
        Assert.Empty(cruder.GetKeys());
    }

    //the details are a sub object: their field opens their own list menu
    [Fact]
    public void GetFieldEditMenuItem_WhenCalled_OpensTheDetailsAsASubMenu()
    {
        // Arrange
        var smartSchemaCruder = new SmartSchemaCruder(_parametersManager.Object, []);
        var sut = new SmartSchemaDetailsFieldEditor(nameof(SmartSchema.Details), _parametersManager.Object);

        // Act
        CliMenuCommand command = sut.GetFieldEditMenuItem(new SmartSchema(), smartSchemaCruder, "Daily");

        // Assert
        Assert.IsType<SubObjectFieldEditorCliMenuCommand>(command);
    }

    //the status lists every detail as "period-count", separated by "/"
    [Fact]
    public void GetValueStatus_WhenThereAreDetails_ListsThemInTheirOrder()
    {
        // Arrange
        var smartSchema = new SmartSchema
        {
            Details =
            {
                new SmartSchemaDetail { PeriodType = EPeriodType.Day, PreserveCount = 3 },
                new SmartSchemaDetail { PeriodType = EPeriodType.Week, PreserveCount = 2 },
                new SmartSchemaDetail { PeriodType = EPeriodType.Month, PreserveCount = 12 }
            }
        };
        var sut = new SmartSchemaDetailsFieldEditor(nameof(SmartSchema.Details));

        // Act
        string status = sut.GetValueStatus(smartSchema);

        // Assert
        Assert.Equal("Day-3/Week-2/Month-12", status);
    }

    [Fact]
    public void GetValueStatus_WhenThereIsOneDetail_ShowsIt()
    {
        // Arrange
        var smartSchema = new SmartSchema
        {
            Details = { new SmartSchemaDetail { PeriodType = EPeriodType.Day, PreserveCount = 3 } }
        };
        var sut = new SmartSchemaDetailsFieldEditor(nameof(SmartSchema.Details));

        // Act
        string status = sut.GetValueStatus(smartSchema);

        // Assert
        Assert.Equal("Day-3", status);
    }

    [Fact]
    public void GetValueStatus_WhenThereAreNoDetails_SaysSo()
    {
        // Arrange
        var sut = new SmartSchemaDetailsFieldEditor(nameof(SmartSchema.Details));

        // Act
        string status = sut.GetValueStatus(new SmartSchema());

        // Assert
        Assert.Equal("No Details", status);
    }

    [Fact]
    public void GetValueStatus_WhenThereIsNoRecord_SaysThereAreNoDetails()
    {
        // Arrange
        var sut = new SmartSchemaDetailsFieldEditor(nameof(SmartSchema.Details));

        // Act
        string status = sut.GetValueStatus(null);

        // Assert
        Assert.Equal("No Details", status);
    }
}
