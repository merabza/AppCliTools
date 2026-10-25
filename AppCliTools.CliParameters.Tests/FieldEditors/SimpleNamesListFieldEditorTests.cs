using System.Collections.Generic;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.CliParameters.Cruders;
using AppCliTools.CliParameters.FieldEditors;
using SystemTools.SystemToolsShared;
using Xunit;

namespace AppCliTools.CliParameters.Tests.FieldEditors;

public sealed class SimpleNamesListFieldEditorTests
{
    private List<string>? _cruderList;

    private SimpleNamesListFieldEditor<TestListCruder> CreateSut()
    {
        return new SimpleNamesListFieldEditor<TestListCruder>(nameof(TestRecord.Names), x =>
        {
            _cruderList = x;
            return new TestListCruder(x);
        });
    }

    //a new record of a cruder gets the default of every field editor, so a null default would replace the list
    //that the model creates
    [Fact]
    public void SetDefault_WhenCalled_SetsAnEmptyList()
    {
        // Arrange
        var record = new TestRecord { Names = null };

        // Act
        CreateSut().SetDefault(record);

        // Assert
        Assert.NotNull(record.Names);
        Assert.Empty(record.Names);
    }

    //the names added through the list menu must reach the record, so the cruder gets the list of the record
    [Fact]
    public void GetSubMenu_WhenTheListIsNull_GivesTheCruderANewListOfTheRecord()
    {
        // Arrange
        var record = new TestRecord { Names = null };

        // Act
        CreateSut().GetSubMenu(record);

        // Assert
        Assert.NotNull(record.Names);
        Assert.Same(record.Names, _cruderList);
    }

    [Fact]
    public void GetSubMenu_WithAList_GivesTheCruderThatList()
    {
        // Arrange
        List<string> names = ["First"];
        var record = new TestRecord { Names = names };

        // Act
        CreateSut().GetSubMenu(record);

        // Assert
        Assert.Same(names, _cruderList);
        Assert.Same(names, record.Names);
    }

    //a list is filled in its own menu after the record is created, not during the creation
    [Fact]
    public void Constructor_WhenCalled_IsNotAskedOnCreate()
    {
        // Act
        SimpleNamesListFieldEditor<TestListCruder> sut = CreateSut();

        // Assert
        Assert.False(sut.EnterFieldDataOnCreate);
    }

    [Fact]
    public void GetFieldEditMenuItem_WhenCalled_OpensTheListAsASubMenu()
    {
        // Arrange
        var record = new TestRecord { Names = [] };

        // Act
        CliMenuCommand result = CreateSut().GetFieldEditMenuItem(record, new TestListCruder([]), "record");

        // Assert
        Assert.IsType<SubObjectFieldEditorCliMenuCommand>(result);
    }

    [Fact]
    public void GetValueStatus_WithoutList_ShowsNoDetails()
    {
        // Act
        string result = CreateSut().GetValueStatus(new TestRecord { Names = null });

        // Assert
        Assert.Equal("No Details", result);
    }

    [Fact]
    public void GetValueStatus_WithEmptyList_ShowsNoDetails()
    {
        // Act
        string result = CreateSut().GetValueStatus(new TestRecord { Names = [] });

        // Assert
        Assert.Equal("No Details", result);
    }

    [Fact]
    public void GetValueStatus_WithOneName_ShowsTheName()
    {
        // Act
        string result = CreateSut().GetValueStatus(new TestRecord { Names = ["First"] });

        // Assert
        Assert.Equal("First", result);
    }

    [Fact]
    public void GetValueStatus_WithTwoNames_ShowsTheirCount()
    {
        // Act
        string result = CreateSut().GetValueStatus(new TestRecord { Names = ["First", "Second"] });

        // Assert
        Assert.Equal("2 Details", result);
    }

    private sealed class TestRecord : ItemData
    {
        public List<string>? Names { get; set; }
    }

    private sealed class TestListCruder : SimpleNamesListCruder
    {
        private readonly List<string> _names;

        public TestListCruder(List<string> names) : base("Name", "Names")
        {
            _names = names;
        }

        protected override List<string> GetList()
        {
            return _names;
        }
    }
}
