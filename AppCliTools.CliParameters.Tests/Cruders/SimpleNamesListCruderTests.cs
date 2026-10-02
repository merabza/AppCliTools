using System.Collections.Generic;
using System.Threading.Tasks;
using AppCliTools.CliParameters.Cruders;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParameters.Tests.Cruders;

//სია პარამეტრების ნაწილია: ცვლილება parametersManager-ის ძირეული ობიექტის შენახვით სრულდება
public sealed class SimpleNamesListCruderTests
{
    [Fact]
    public async Task Save_WhenCreatedWithParametersManager_SavesTheRootParameters()
    {
        // Arrange
        var parametersManager = new RecordingParametersManager();
        var sut = new TestListCruder(parametersManager, ["First"]);

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
        var sut = new TestListCruder(["First"]);

        // Act
        bool result = await sut.Save("Saved");

        // Assert
        Assert.True(result);
    }

    //ჩანაწერის ცვლილება თვითონ ფაილს არ წერს: ოპერაციას ბრძანება ერთი Save-ით ასრულებს
    [Fact]
    public async Task ChangeRecordKey_WhenCalled_ChangesTheListWithoutSaving()
    {
        // Arrange
        var parametersManager = new RecordingParametersManager();
        List<string> names = ["First", "Second"];
        var sut = new TestListCruder(parametersManager, names);

        // Act
        bool result = await sut.ChangeRecordKey("First", "Renamed");

        // Assert
        Assert.True(result);
        Assert.Equal(["Second", "Renamed"], names);
        Assert.Empty(parametersManager.Saves);
    }

    [Fact]
    public void GetKeys_WhenCalled_ReturnsTheNamesInOrder()
    {
        // Arrange
        var sut = new TestListCruder(["Second", "First"]);

        // Act
        List<string> keys = sut.GetKeys();

        // Assert
        Assert.Equal(["First", "Second"], keys);
    }

    [Fact]
    public void GetCrudersDictionary_WhenCalled_WrapsEveryNameIntoATextItem()
    {
        // Arrange
        var sut = new TestListCruder(["First"]);

        // Act
        Dictionary<string, ItemData> items = sut.Items;

        // Assert
        KeyValuePair<string, ItemData> item = Assert.Single(items);
        Assert.Equal("First", item.Key);
        Assert.Equal("First", Assert.IsType<TextItemData>(item.Value).Text);
    }

    [Theory]
    [InlineData("First", true)]
    [InlineData("first", false)]
    [InlineData("Missing", false)]
    public void ContainsRecordWithKey_WhenCalled_LooksForTheExactName(string recordKey, bool expected)
    {
        // Arrange
        var sut = new TestListCruder(["First", "Second"]);

        // Act
        bool result = sut.ContainsRecordWithKey(recordKey);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task RemoveRecordWithKey_WhenCalled_RemovesTheName()
    {
        // Arrange
        List<string> names = ["First", "Second"];
        var sut = new TestListCruder(names);

        // Act
        await sut.Remove("First");

        // Assert
        Assert.Equal(["Second"], names);
    }

    [Fact]
    public async Task AddRecordWithKey_WhenCalled_AppendsTheName()
    {
        // Arrange
        List<string> names = ["First"];
        var sut = new TestListCruder(names);

        // Act
        await sut.Add("Second", new TextItemData());

        // Assert
        Assert.Equal(["First", "Second"], names);
    }

    [Fact]
    public void CreateNewItem_WhenCalled_ReturnsAnEmptyTextItem()
    {
        // Arrange
        var sut = new TestListCruder([]);

        // Act
        ItemData item = sut.NewItem();

        // Assert
        Assert.Null(Assert.IsType<TextItemData>(item).Text);
    }

    [Fact]
    public void GetStatusFor_WhenCalled_ReturnsNoStatus()
    {
        // Arrange
        var sut = new TestListCruder(["First"]);

        // Act
        string? status = sut.GetStatusFor("First");

        // Assert
        Assert.Null(status);
    }

    private sealed class TestListCruder : SimpleNamesListCruder
    {
        private readonly List<string> _names;

        public TestListCruder(List<string> names) : base("Name", "Names")
        {
            _names = names;
        }

        public TestListCruder(IParametersManager parametersManager, List<string> names) : base(parametersManager,
            "Name", "Names")
        {
            _names = names;
        }

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

        protected override List<string> GetList()
        {
            return _names;
        }
    }
}
