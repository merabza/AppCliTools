using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParameters.Tests;

//იმახსოვრებს ყოველ Save-ს: რომელი ობიექტი და რა შეტყობინებით შეინახა
internal sealed class RecordingParametersManager : IParametersManager
{
    public List<(IParameters Parameters, string Message)> Saves { get; } = [];

    public IParameters Parameters { get; set; } = new RootParameters();

    public string? ParametersFileName => null;

    public ValueTask<bool> Save(IParameters parameters, string message, string? saveAsFilePath = null,
        CancellationToken cancellationToken = default)
    {
        Saves.Add((parameters, message));
        return ValueTask.FromResult(true);
    }

    private sealed class RootParameters : IParameters
    {
        public bool CheckBeforeSave()
        {
            return true;
        }
    }
}
