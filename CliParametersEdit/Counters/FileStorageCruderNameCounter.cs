using CliParametersEdit.Cruders;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParametersEdit.Counters;

//გამოიყენება ApAgent პროექტში
// ReSharper disable once UnusedType.Global
public sealed class FileStorageCruderNameCounter
{
    private readonly string? _currentName;
    private readonly string _fieldName;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public FileStorageCruderNameCounter(ILogger logger, IParametersManager parametersManager, string fieldName,
        string? currentName)
    {
        _logger = logger;
        _parametersManager = parametersManager;
        _fieldName = fieldName;
        _currentName = currentName;
    }

    public string? Count()
    {
        var fileStorageCruder = FileStorageCruder.Create(_logger, _parametersManager);
        return fileStorageCruder.GetNameWithPossibleNewName(_fieldName, _currentName);
    }
}