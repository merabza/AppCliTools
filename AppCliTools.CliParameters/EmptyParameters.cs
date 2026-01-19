using ParametersManagement.LibParameters;

namespace CliParameters;

public sealed class EmptyParameters : IParameters
{
    public bool CheckBeforeSave()
    {
        return false;
    }
}
