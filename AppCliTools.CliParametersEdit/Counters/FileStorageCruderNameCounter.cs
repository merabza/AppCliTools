using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParametersEdit.Cruders;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParametersEdit.Counters;

//გამოიყენება Replicator პროექტში
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

    public ValueTask<string?> Count(CancellationToken cancellationToken = default)
    {
        var fileStorageCruder = FileStorageCruder.Create(_logger, _parametersManager);
        return fileStorageCruder.GetNameWithPossibleNewName(_fieldName, _currentName, null, false, cancellationToken);
    }
}
