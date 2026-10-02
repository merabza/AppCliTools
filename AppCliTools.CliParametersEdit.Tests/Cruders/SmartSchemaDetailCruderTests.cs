using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliParametersEdit.Cruders;
using Moq;
using ParametersManagement.LibFileParameters.Interfaces;
using ParametersManagement.LibFileParameters.Models;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersEdit.Tests.Cruders;

//Smart Schema-ს დეტალები პარამეტრების ნაწილია: ცვლილება ძირეული ობიექტის შენახვით სრულდება
public sealed class SmartSchemaDetailCruderTests
{
    private readonly Mock<IParametersWithSmartSchemas> _parameters = new();
    private readonly Mock<IParametersManager> _parametersManager = new();

    public SmartSchemaDetailCruderTests()
    {
        _parametersManager.SetupGet(x => x.Parameters).Returns(_parameters.Object);
        _parametersManager.Setup(x => x.Save(It.IsAny<IParameters>(), It.IsAny<string>(), It.IsAny<string?>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task Save_WhenCreatedWithParametersManager_SavesTheRootParameters()
    {
        // Arrange
        var sut = new SmartSchemaDetailCruder(_parametersManager.Object, []);

        // Act
        bool result = await sut.Save("Saved");

        // Assert
        Assert.True(result);
        _parametersManager.Verify(x => x.Save(_parameters.Object, "Saved", null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    //ძველი კონსტრუქტორი სხვა აპლიკაციებისთვის რჩება და, როგორც ადრე, არაფერს ინახავს
    [Fact]
    public async Task Save_WhenCreatedWithoutParametersManager_ReturnsTrue()
    {
        // Arrange
        var sut = new SmartSchemaDetailCruder([]);

        // Act
        bool result = await sut.Save("Saved");

        // Assert
        Assert.True(result);
    }

    //ჩანაწერის ცვლილება თვითონ ფაილს არ წერს: ოპერაციას ბრძანება ერთი Save-ით ასრულებს
    [Fact]
    public async Task UpdateRecordWithKey_WhenCalled_ChangesTheDetailWithoutSaving()
    {
        // Arrange
        List<SmartSchemaDetail> details = [new() { PeriodType = EPeriodType.Day, PreserveCount = 3 }];
        var sut = new SmartSchemaDetailCruder(_parametersManager.Object, details);

        // Act
        await sut.UpdateRecordWithKey(nameof(EPeriodType.Day),
            new SmartSchemaDetail { PeriodType = EPeriodType.Day, PreserveCount = 7 });

        // Assert
        Assert.Equal(7, Assert.Single(details).PreserveCount);
        _parametersManager.Verify(
            x => x.Save(It.IsAny<IParameters>(), It.IsAny<string>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    //a detail is edited by its period type and the count of copies to keep
    [Fact]
    public void Constructor_WhenCalled_EditsThePeriodTypeAndThePreserveCount()
    {
        // Act
        var sut = new SmartSchemaDetailCruder([]);

        // Assert
        Assert.Equal("Smart Schema Detail", sut.CrudName);
        Assert.Equal("Smart Schema Details", sut.CrudNamePlural);
        List<FieldEditor> fieldEditors = CliMenuTestAccess.GetFieldEditors(sut);
        Assert.Equal([nameof(SmartSchemaDetail.PeriodType), nameof(SmartSchemaDetail.PreserveCount)],
            fieldEditors.Select(x => x.PropertyName));
        Assert.IsType<EnumFieldEditor<EPeriodType>>(fieldEditors[0]);
        Assert.IsType<IntFieldEditor>(fieldEditors[1]);
    }

    [Fact]
    public void GetKeys_WhenCalled_ReturnsThePeriodTypesInOrder()
    {
        // Arrange
        var sut = new SmartSchemaDetailCruder([Detail(EPeriodType.Week, 2), Detail(EPeriodType.Day, 3)]);

        // Act
        List<string> keys = sut.GetKeys();

        // Assert
        Assert.Equal([nameof(EPeriodType.Day), nameof(EPeriodType.Week)], keys);
    }

    [Fact]
    public void GetCrudersDictionary_WhenCalled_KeysTheDetailsByTheirPeriodType()
    {
        // Arrange
        SmartSchemaDetail day = Detail(EPeriodType.Day, 3);
        var sut = new SmartSchemaDetailCruder([day]);

        // Act
        Dictionary<string, ItemData> items = CliMenuTestAccess.GetCrudersDictionary(sut);

        // Assert
        Assert.Same(day, Assert.Single(items, x => x.Key == nameof(EPeriodType.Day)).Value);
    }

    [Theory]
    [InlineData(nameof(EPeriodType.Day), true)]
    [InlineData(nameof(EPeriodType.Week), false)]
    public void ContainsRecordWithKey_WhenCalled_LooksForThePeriodType(string recordKey, bool expected)
    {
        // Arrange
        var sut = new SmartSchemaDetailCruder([Detail(EPeriodType.Day, 3)]);

        // Act
        bool result = sut.ContainsRecordWithKey(recordKey);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task RemoveRecordWithKey_WhenPeriodTypeExists_RemovesItsDetail()
    {
        // Arrange
        List<SmartSchemaDetail> details = [Detail(EPeriodType.Day, 3), Detail(EPeriodType.Week, 2)];
        var sut = new SmartSchemaDetailCruder(details);

        // Act
        await CliMenuTestAccess.InvokeRemoveRecordWithKey(sut, nameof(EPeriodType.Day));

        // Assert
        Assert.Equal(EPeriodType.Week, Assert.Single(details).PeriodType);
    }

    [Fact]
    public async Task RemoveRecordWithKey_WhenPeriodTypeIsMissing_ChangesNothing()
    {
        // Arrange
        List<SmartSchemaDetail> details = [Detail(EPeriodType.Day, 3)];
        var sut = new SmartSchemaDetailCruder(details);

        // Act
        await CliMenuTestAccess.InvokeRemoveRecordWithKey(sut, nameof(EPeriodType.Week));

        // Assert
        Assert.Equal(EPeriodType.Day, Assert.Single(details).PeriodType);
    }

    //the key is the period type: a record of another period or of another type cannot update it
    [Fact]
    public async Task UpdateRecordWithKey_WhenRecordHasAnotherPeriodType_ChangesNothing()
    {
        // Arrange
        List<SmartSchemaDetail> details = [Detail(EPeriodType.Day, 3)];
        var sut = new SmartSchemaDetailCruder(details);

        // Act
        await sut.UpdateRecordWithKey(nameof(EPeriodType.Day), Detail(EPeriodType.Week, 7));

        // Assert
        Assert.Equal(3, Assert.Single(details).PreserveCount);
    }

    [Fact]
    public async Task UpdateRecordWithKey_WhenRecordIsNotADetail_ChangesNothing()
    {
        // Arrange
        List<SmartSchemaDetail> details = [Detail(EPeriodType.Day, 3)];
        var sut = new SmartSchemaDetailCruder(details);

        // Act
        await sut.UpdateRecordWithKey(nameof(EPeriodType.Day), new ItemData());

        // Assert
        Assert.Equal(3, Assert.Single(details).PreserveCount);
    }

    [Fact]
    public async Task UpdateRecordWithKey_WhenPeriodTypeIsMissing_AddsNothing()
    {
        // Arrange
        List<SmartSchemaDetail> details = [Detail(EPeriodType.Day, 3)];
        var sut = new SmartSchemaDetailCruder(details);

        // Act
        await sut.UpdateRecordWithKey(nameof(EPeriodType.Week), Detail(EPeriodType.Week, 7));

        // Assert
        Assert.Equal(EPeriodType.Day, Assert.Single(details).PeriodType);
    }

    [Fact]
    public async Task AddRecordWithKey_WhenRecordIsADetail_AddsIt()
    {
        // Arrange
        List<SmartSchemaDetail> details = [];
        var sut = new SmartSchemaDetailCruder(details);
        SmartSchemaDetail week = Detail(EPeriodType.Week, 2);

        // Act
        await CliMenuTestAccess.InvokeAddRecordWithKey(sut, nameof(EPeriodType.Week), week);

        // Assert
        Assert.Same(week, Assert.Single(details));
    }

    [Fact]
    public async Task AddRecordWithKey_WhenRecordIsNotADetail_AddsNothing()
    {
        // Arrange
        List<SmartSchemaDetail> details = [];
        var sut = new SmartSchemaDetailCruder(details);

        // Act
        await CliMenuTestAccess.InvokeAddRecordWithKey(sut, nameof(EPeriodType.Week), new ItemData());

        // Assert
        Assert.Empty(details);
    }

    [Fact]
    public void CreateNewItem_WhenCalled_ReturnsANewDetail()
    {
        // Arrange
        var sut = new SmartSchemaDetailCruder([]);

        // Act
        ItemData item = CliMenuTestAccess.InvokeCreateNewItem(sut);

        // Assert
        Assert.Equal(0, Assert.IsType<SmartSchemaDetail>(item).PreserveCount);
    }

    private static SmartSchemaDetail Detail(EPeriodType periodType, int preserveCount)
    {
        return new SmartSchemaDetail { PeriodType = periodType, PreserveCount = preserveCount };
    }
}
