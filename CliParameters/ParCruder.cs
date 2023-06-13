using LibParameters;

namespace CliParameters;

public /*open*/ class ParCruder : Cruder
{
    protected readonly IParametersManager ParametersManager;

    protected ParCruder(IParametersManager parametersManager, string crudName, string crudNamePlural,
        bool fieldNameFromItem = false, bool canEditFieldsInSequence = true) : base(crudName, crudNamePlural,
        fieldNameFromItem, canEditFieldsInSequence)
    {
        ParametersManager = parametersManager;
    }

    public override void Save(string message)
    {
        ParametersManager.Save(ParametersManager.Parameters, message);
    }
}