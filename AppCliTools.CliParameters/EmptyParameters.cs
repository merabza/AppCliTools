using ParametersManagement.LibParameters;

namespace AppCliTools.CliParameters;

public sealed class EmptyParameters : IParameters
{
    public bool CheckBeforeSave()
    {
        return false;
    }
}
