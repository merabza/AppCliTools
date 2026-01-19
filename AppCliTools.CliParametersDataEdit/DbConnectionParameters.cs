using ParametersManagement.LibParameters;

namespace AppCliTools.CliParametersDataEdit;

public /*open*/ class DbConnectionParameters : IParameters
{
    public bool CheckBeforeSave()
    {
        return true;
    }

    public virtual string GetStatus()
    {
        return "(Unknown)";
    }
}
