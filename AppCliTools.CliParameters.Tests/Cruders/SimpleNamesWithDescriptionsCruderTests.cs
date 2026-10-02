using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppCliTools.CliParameters.Cruders;
using AppCliTools.CliParameters.FieldEditors;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParameters.Tests.Cruders;

//ლექსიკონი პარამეტრების ნაწილია: ცვლილება parametersManager-ის ძირეული ობიექტის შენახვით სრულდება
public sealed class SimpleNamesWithDescriptionsCruderTests
{
    [Fact]
    public async Task Save_WhenCreatedWithParametersManager_SavesTheRootParameters()
    {
        // Arrange
        var parametersManager = new RecordingParametersManager();
        var sut = new TestDescriptionsCruder(parametersManager, new Dictionary<string, string> { ["Dev"] = "Dev" });

        // Act
        bool result = await sut.Save("Saved");

        // Assert
        Assert.True(result);
        (IParameters parameters, string message) = Assert.Single(parametersManager.Saves);
        Assert.Same(parametersManager.Parameters, parameters);
        Assert.Equal("Saved", message);
    }

    //ძველი კონსტრუქტორი სხვა აპლიკაციებისთვის რჩება და, როგორც ადრე, არაფერს ინახავს
    [Fact]
    public async Task Save_WhenCreatedWithoutParametersManager_ReturnsTrue()
    {
        // Arrange
        var sut = new TestDescriptionsCruder([]);

        // Act
        bool result = await sut.Save("Saved");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Constructor_WhenCreatedWithParametersManager_AddsTheDescriptionEditor()
    {
        // Act
        var sut = new TestDescriptionsCruder(new RecordingParametersManager(), []);

        // Assert
        FieldEditor descriptionEditor = Assert.Single(sut.Editors);
        Assert.IsType<OptionalTextFieldEditor>(descriptionEditor);
        Assert.Equal(nameof(TextItemData.Text), descriptionEditor.PropertyName);
        Assert.True(descriptionEditor.EnterFieldDataOnCreate);
    }

    //ჩანაწერის ცვლილება თვითონ ფაილს არ წერს: ოპერაციას ბრძანება ერთი Save-ით ასრულებს
    [Fact]
    public async Task UpdateRecordWithKey_WhenCalled_ChangesTheDescriptionWithoutSaving()
    {
        // Arrange
        var parametersManager = new RecordingParametersManager();
        var descriptions = new Dictionary<string, string> { ["Dev"] = "Development" };
        var sut = new TestDescriptionsCruder(parametersManager, descriptions);

        // Act
        await sut.UpdateRecordWithKey("Dev", new TextItemData { Text = "Developer machines" });

        // Assert
        Assert.Equal("Developer machines", descriptions["Dev"]);
        Assert.Empty(parametersManager.Saves);
    }

    [Fact]
    public async Task UpdateRecordWithKey_WhenRecordIsNotATextItem_Throws()
    {
        // Arrange
        var sut = new TestDescriptionsCruder(new Dictionary<string, string> { ["Dev"] = "Development" });

        // Act
        var exception =
            await Assert.ThrowsAsync<Exception>(async () => await sut.UpdateRecordWithKey("Dev", new ItemData()));

        // Assert
        Assert.Equal("newRecord is null in UpdateRecordWithKey", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task UpdateRecordWithKey_WhenDescriptionIsEmpty_ThrowsAndKeepsTheDescription(string? description)
    {
        // Arrange
        var descriptions = new Dictionary<string, string> { ["Dev"] = "Development" };
        var sut = new TestDescriptionsCruder(descriptions);

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(async () =>
            await sut.UpdateRecordWithKey("Dev", new TextItemData { Text = description }));

        // Assert
        Assert.Equal("newReactAppType.Description is empty in UpdateRecordWithKey", exception.Message);
        Assert.Equal("Development", descriptions["Dev"]);
    }

    [Fact]
    public async Task AddRecordWithKey_WhenCalled_AddsTheNameWithItsDescription()
    {
        // Arrange
        var descriptions = new Dictionary<string, string>();
        var sut = new TestDescriptionsCruder(descriptions);

        // Act
        await sut.Add("Dev", new TextItemData { Text = "Development" });

        // Assert
        Assert.Equal("Development", Assert.Single(descriptions, x => x.Key == "Dev").Value);
    }

    [Fact]
    public async Task AddRecordWithKey_WhenRecordIsNotATextItem_Throws()
    {
        // Arrange
        var sut = new TestDescriptionsCruder([]);

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(async () => await sut.Add("Dev", new ItemData()));

        // Assert
        Assert.Equal("newRecord is null in AddRecordWithKey", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task AddRecordWithKey_WhenDescriptionIsEmpty_ThrowsAndAddsNothing(string? description)
    {
        // Arrange
        var descriptions = new Dictionary<string, string>();
        var sut = new TestDescriptionsCruder(descriptions);

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(async () =>
            await sut.Add("Dev", new TextItemData { Text = description }));

        // Assert
        Assert.Equal("Description is empty in AddRecordWithKey", exception.Message);
        Assert.Empty(descriptions);
    }

    [Fact]
    public async Task RemoveRecordWithKey_WhenCalled_RemovesTheName()
    {
        // Arrange
        var descriptions = new Dictionary<string, string> { ["Dev"] = "Development", ["Prod"] = "Production" };
        var sut = new TestDescriptionsCruder(descriptions);

        // Act
        await sut.Remove("Dev");

        // Assert
        Assert.Equal(["Prod"], descriptions.Keys);
    }

    [Theory]
    [InlineData("Dev", true)]
    [InlineData("Missing", false)]
    public void ContainsRecordWithKey_WhenCalled_LooksForTheName(string recordKey, bool expected)
    {
        // Arrange
        var sut = new TestDescriptionsCruder(new Dictionary<string, string> { ["Dev"] = "Development" });

        // Act
        bool result = sut.ContainsRecordWithKey(recordKey);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetCrudersDictionary_WhenCalled_WrapsEveryDescriptionIntoATextItem()
    {
        // Arrange
        var sut = new TestDescriptionsCruder(new Dictionary<string, string> { ["Dev"] = "Development" });

        // Act
        Dictionary<string, ItemData> items = sut.Items;

        // Assert
        KeyValuePair<string, ItemData> item = Assert.Single(items);
        Assert.Equal("Dev", item.Key);
        Assert.Equal("Development", Assert.IsType<TextItemData>(item.Value).Text);
    }

    [Fact]
    public void CreateNewItem_WhenCalled_ReturnsAnEmptyTextItem()
    {
        // Arrange
        var sut = new TestDescriptionsCruder([]);

        // Act
        ItemData item = sut.NewItem();

        // Assert
        Assert.Null(Assert.IsType<TextItemData>(item).Text);
    }

    //the status of a name is its description
    [Theory]
    [InlineData("Dev", "Development")]
    [InlineData("Empty", null)]
    [InlineData("Blank", null)]
    [InlineData("Missing", null)]
    public void GetStatusFor_WhenCalled_ReturnsTheDescriptionIfThereIsOne(string name, string? expected)
    {
        // Arrange
        var sut = new TestDescriptionsCruder(
            new Dictionary<string, string> { ["Dev"] = "Development", ["Empty"] = "", ["Blank"] = " " });

        // Act
        string? status = sut.GetStatusFor(name);

        // Assert
        Assert.Equal(expected, status);
    }

    //the description field may have another name, for example "Path"
    [Fact]
    public void Constructor_WhenDescriptionHasItsOwnName_ShowsThatName()
    {
        // Arrange
        var sut = new TestDescriptionsCruder(new RecordingParametersManager(),
            new Dictionary<string, string> { ["Dev"] = "Development" }, "Path");

        // Act
        List<string> commandNames = [.. sut.GetDetailsSubMenu("Dev").Select(x => x.Name)];

        // Assert
        Assert.Equal(["Record Name", "Path"], commandNames);
    }

    private sealed class TestDescriptionsCruder : SimpleNamesWithDescriptionsCruder
    {
        private readonly Dictionary<string, string> _descriptions;

        public TestDescriptionsCruder(Dictionary<string, string> descriptions) : base("Environment", "Environments")
        {
            _descriptions = descriptions;
        }

        public TestDescriptionsCruder(IParametersManager parametersManager, Dictionary<string, string> descriptions) :
            base(parametersManager, "Environment", "Environments")
        {
            _descriptions = descriptions;
        }

        public TestDescriptionsCruder(IParametersManager parametersManager, Dictionary<string, string> descriptions,
            string descriptionFieldRealName) : base(parametersManager, "Environment", "Environments",
            descriptionFieldRealName)
        {
            _descriptions = descriptions;
        }

        public IReadOnlyList<FieldEditor> Editors => FieldEditors;

        public Dictionary<string, ItemData> Items => GetCrudersDictionary();

        public ItemData NewItem()
        {
            return CreateNewItem(null, null);
        }

        public ValueTask Remove(string recordKey)
        {
            return RemoveRecordWithKey(recordKey);
        }

        public ValueTask Add(string recordKey, ItemData newRecord)
        {
            return AddRecordWithKey(recordKey, newRecord);
        }

        protected override Dictionary<string, string> GetDictionary()
        {
            return _descriptions;
        }
    }
}
