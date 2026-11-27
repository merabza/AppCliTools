using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.FieldEditors;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParameters.Tests;

//the manager that an editor of a sub object (e.g. the database parameters of a project) saves through: the sub object
//is a field of a record inside the parent parameters, and every save writes the whole parent
public sealed class SubParametersManagerTests
{
    private const string SaveAsFilePath = "Copy.json";

    private readonly RecordingParametersManager _parentParametersManager = new();
    private readonly TestRecord _record = new();
    private readonly SubParameters _subParameters = new();
    private readonly SubParametersManager<SubParameters> _sut;

    public SubParametersManagerTests()
    {
        _record.Sub = _subParameters;
        _sut = new SubParametersManager<SubParameters>(_subParameters, _parentParametersManager,
            new SubParametersFieldEditor(), _record);
    }

    [Fact]
    public void Constructor_KeepsTheSubObjectAsItsParametersWithoutAFileOfItsOwn()
    {
        // Assert
        Assert.Same(_subParameters, _sut.Parameters);
        Assert.Null(_sut.ParametersFileName);
    }

    [Fact]
    public async Task Save_WhenGivenTheSubObjectType_PutsItIntoTheRecordAndSavesTheParent()
    {
        // Arrange
        var newSubParameters = new SubParameters();

        // Act
        bool result = await _sut.Save(newSubParameters, "Saved", SaveAsFilePath, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Same(newSubParameters, _record.Sub);
        Assert.Same(newSubParameters, _sut.Parameters);
        (IParameters parameters, string message) = Assert.Single(_parentParametersManager.Saves);
        Assert.Same(_parentParametersManager.Parameters, parameters);
        Assert.Equal("Saved", message);
        Assert.Equal(SaveAsFilePath, Assert.Single(_parentParametersManager.SaveAsFilePaths));
    }

    [Fact]
    public async Task Save_WhenGivenAnotherParametersType_ReturnsFalseWithoutChangingTheRecord()
    {
        // Act
        bool result = await _sut.Save(new OtherParameters(), "Saved");

        // Assert
        Assert.False(result);
        Assert.Same(_subParameters, _record.Sub);
        Assert.Empty(_parentParametersManager.Saves);
    }

    [Fact]
    public async Task Save_WhenTheParentSaveFails_ReturnsFalse()
    {
        // Arrange
        _parentParametersManager.SaveResult = false;

        // Act
        bool result = await _sut.Save(new SubParameters(), "Saved");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Delete_RemovesTheSubObjectFromTheRecordAndSavesTheParent()
    {
        // Act
        bool result = await _sut.Delete("Deleted", CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Null(_record.Sub);
        (IParameters parameters, string message) = Assert.Single(_parentParametersManager.Saves);
        Assert.Same(_parentParametersManager.Parameters, parameters);
        Assert.Equal("Deleted", message);
        Assert.Null(Assert.Single(_parentParametersManager.SaveAsFilePaths));
    }

    [Fact]
    public async Task Delete_WhenTheParentSaveFails_ReturnsFalse()
    {
        // Arrange
        _parentParametersManager.SaveResult = false;

        // Act
        bool result = await _sut.Delete("Deleted");

        // Assert
        Assert.False(result);
        Assert.Null(_record.Sub);
    }

    private sealed class TestRecord
    {
        public SubParameters? Sub { get; set; }
    }

    private sealed class SubParameters : IParameters
    {
        public bool CheckBeforeSave()
        {
            return true;
        }
    }

    private sealed class OtherParameters : IParameters
    {
        public bool CheckBeforeSave()
        {
            return true;
        }
    }

    //the field of the record that holds the sub object
    private sealed class SubParametersFieldEditor : FieldEditor<SubParameters>
    {
        public SubParametersFieldEditor() : base(nameof(TestRecord.Sub))
        {
        }
    }
}
