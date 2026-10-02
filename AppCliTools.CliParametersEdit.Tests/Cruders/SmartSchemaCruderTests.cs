using System.Collections.Generic;
using System.Linq;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliParametersEdit.CliMenuCommands;
using AppCliTools.CliParametersEdit.Cruders;
using AppCliTools.CliParametersEdit.FieldEditors;
using Moq;
using ParametersManagement.LibFileParameters.Interfaces;
using ParametersManagement.LibFileParameters.Models;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParametersEdit.Tests.Cruders;

public sealed class SmartSchemaCruderTests
{
    private readonly Mock<IParametersWithSmartSchemas> _parameters = new();
    private readonly Mock<IParametersManager> _parametersManager = new();
    private readonly Dictionary<string, SmartSchema> _smartSchemas = new() { ["Daily"] = new SmartSchema() };

    public SmartSchemaCruderTests()
    {
        _parameters.SetupGet(x => x.SmartSchemas).Returns(_smartSchemas);
        _parametersManager.SetupGet(x => x.Parameters).Returns(_parameters.Object);
    }

    //a schema is edited by the count of the newest copies to keep and by its details
    [Fact]
    public void Constructor_WhenCalled_EditsLastPreserveCountAndDetails()
    {
        // Act
        var sut = new SmartSchemaCruder(_parametersManager.Object, _smartSchemas);

        // Assert
        List<FieldEditor> fieldEditors = CliMenuTestAccess.GetFieldEditors(sut);
        Assert.Equal([nameof(SmartSchema.LastPreserveCount), nameof(SmartSchema.Details)],
            fieldEditors.Select(x => x.PropertyName));
        Assert.IsType<IntFieldEditor>(fieldEditors[0]);
        Assert.IsType<SmartSchemaDetailsFieldEditor>(fieldEditors[1]);
    }

    [Fact]
    public void Constructor_WhenCalled_NamesTheRecordsSmartSchemas()
    {
        // Act
        var sut = new SmartSchemaCruder(_parametersManager.Object, _smartSchemas);

        // Assert
        Assert.Equal("Smart Schema", sut.CrudName);
        Assert.Equal("Smart Schemas", sut.CrudNamePlural);
    }

    [Fact]
    public void Create_WhenCalled_EditsTheSmartSchemasOfTheParameters()
    {
        // Act
        var sut = SmartSchemaCruder.Create(_parametersManager.Object);

        // Assert
        Assert.Equal(["Daily"], sut.GetKeys());
    }

    [Fact]
    public void GetListMenu_WhenCalled_OffersToGenerateTheStandardSchemas()
    {
        // Arrange
        var sut = new SmartSchemaCruder(_parametersManager.Object, _smartSchemas);

        // Act
        CliMenuSet listMenu = sut.GetListMenu();

        // Assert
        Assert.Single(CliMenuTestAccess.GetMenuItems(listMenu),
            x => x.CliMenuCommand is GenerateStandardSmartSchemasCliMenuCommand);
    }
}
