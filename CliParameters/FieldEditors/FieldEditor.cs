using CliMenu;
using CliParameters.MenuCommands;
using LibDataInput;
using LibParameters;
using SystemToolsShared;

namespace CliParameters.FieldEditors;

public /*open*/ class FieldEditor
{
    private readonly bool _isSubObject;
    protected readonly string FieldName;
    public readonly string PropertyName;
    public bool Enabled = true;

    protected FieldEditor(string propertyName, string? propertyDescriptor, bool isSubObject)
    {
        PropertyName = propertyName;
        _isSubObject = isSubObject;
        FieldName = propertyDescriptor ?? PropertyName.SplitWithSpacesCamelParts();
    }

    public virtual void UpdateField(string? recordKey, object recordForUpdate)
    {
    }

    public void AddParameterEditMenuItem(CliMenuSet menuSet, ParametersEditor parametersEditor)
    {
        if (_isSubObject)
            menuSet.AddMenuItem(new ParameterSubObjectFieldEditorMenuCommand(FieldName, this, parametersEditor));
        else
            menuSet.AddMenuItem(new ParameterFieldEditorMenuCommand(FieldName, this, parametersEditor));
    }

    public void AddFieldEditMenuItem(CliMenuSet menuSet, ItemData recordForUpdate, Cruder cruder,
        string recordKey)
    {
        if (_isSubObject)
            menuSet.AddMenuItem(new SubObjectFieldEditorMenuCommand(FieldName, this, recordForUpdate));
        else
            menuSet.AddMenuItem(new FieldEditorMenuCommand(FieldName, this, recordForUpdate, cruder, recordKey));
    }

    public virtual CliMenuSet? GetSubMenu(object record)
    {
        return null;
    }

    public virtual string GetValueStatus(object? record)
    {
        return "Unknown";
    }

    protected static T? GetValue<T>(object record, string propertyName)
    {
        var currentRecordPropertyInfo = record.GetType().GetProperty(propertyName);
        return currentRecordPropertyInfo is null
            ? throw new DataInputException($"property {propertyName} not found")
            : (T?)currentRecordPropertyInfo.GetValue(record);
    }

    public virtual void SetDefault(ItemData currentItem)
    {
    }
}

public /*open*/ class FieldEditor<T> : FieldEditor
{
    protected FieldEditor(string propertyName, string? propertyDescriptor = null, bool isSubObject = false) : base(
        propertyName, propertyDescriptor, isSubObject)
    {
    }

    //აქ public საჭიროა CliParametersDataEdit.ConnectionStringParametersManager.Save მეთოდისათვის
    public void SetValue(object record, T? value)
    {
        var recordForUpdatePropertyInfo = record.GetType().GetProperty(PropertyName);
        if (recordForUpdatePropertyInfo is null)
            throw new DataInputException($"property {PropertyName} not found in record for update");

        recordForUpdatePropertyInfo.SetValue(record, value, null);
    }

    public override string GetValueStatus(object? record)
    {
        var val = GetValue(record);
        return val is null ? "" : val.ToString() ?? "";
    }

    protected T? GetValue(object? record, T? defaultValue = default)
    {
        if (record == null)
            return defaultValue;
        var currentRecordPropertyInfo = record.GetType().GetProperty(PropertyName);
        if (currentRecordPropertyInfo == null) throw new DataInputException($"property {PropertyName} not found");

        var toRet = (T?)currentRecordPropertyInfo.GetValue(record);
        return toRet ?? defaultValue;
    }

    //protected T? GetValue(object? record, bool throwException = true, T? defaultValue = default)
    //{
    //    if (record == null)
    //        return defaultValue;
    //    var currentRecordPropertyInfo = record.GetType().GetProperty(PropertyName);
    //    if (currentRecordPropertyInfo == null)
    //    {
    //        if (throwException)
    //            throw new DataInputException($"property {PropertyName} not found");
    //        return defaultValue;
    //    }

    //    var toRet = (T?)currentRecordPropertyInfo.GetValue(record);
    //    return toRet ?? defaultValue;
    //}
}