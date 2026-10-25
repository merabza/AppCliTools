using System.Threading;
using System.Threading.Tasks;
using AppCliTools.LibDataInput;

namespace AppCliTools.CliParameters.FieldEditors;

public sealed class IntFieldEditor : FieldEditor<int>
{
    //ნაგულისხმევი მნიშვნელობა base-ს გადაეცემა: SetDefault ახალ ჩანაწერს მას აძლევს და არა 0-ს
    public IntFieldEditor(string propertyName, int defaultValue = 0, bool enterFieldDataOnCreate = false) : base(
        propertyName, enterFieldDataOnCreate, defaultValue)
    {
    }

    public override ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        SetValue(recordForUpdate, Inputer.InputInt(FieldName, GetValue(recordForUpdate, DefaultValue)));
        return ValueTask.CompletedTask;
    }
}
