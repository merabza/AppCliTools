using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParameters.Tests;

//იმახსოვრებს ყოველ Save-ს: რომელი ობიექტი, რა შეტყობინებით და რა ფაილის სახელით შეინახა. აბრუნებს SaveResult-ს
internal sealed class RecordingParametersManager : IParametersManager
{
    public List<(IParameters Parameters, string Message)> Saves { get; } = [];

    public List<string?> SaveAsFilePaths { get; } = [];

    public bool SaveResult { get; set; } = true;

    public IParameters Parameters { get; set; } = new RootParameters();

    public string? ParametersFileName => null;

    public ValueTask<bool> Save(IParameters parameters, string message, string? saveAsFilePath = null,
        CancellationToken cancellationToken = default)
    {
        Saves.Add((parameters, message));
        SaveAsFilePaths.Add(saveAsFilePath);
        return ValueTask.FromResult(SaveResult);
    }

    private sealed class RootParameters : IParameters
    {
        public bool CheckBeforeSave()
        {
            return true;
        }
    }
}
