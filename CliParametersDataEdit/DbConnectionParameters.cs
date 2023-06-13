using LibParameters;

namespace CliParametersDataEdit;

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