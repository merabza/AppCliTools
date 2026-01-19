using System.Reflection;
using CliMenu;
using CliParameters.CliMenuCommands;
using CliParameters.Cruders;
using LibDataInput;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace CliParameters.FieldEditors;

public /*open*/ class FieldEditor
{
    private readonly bool _isSubObject;

    public readonly bool
        EnterFieldDataOnCreate; //ეს ველი მიუთითებს გამოიტანოს თუ არა შექმნისას შეთავაზება ამ ველის მნიშვნელობის შესავსებად

    protected readonly string FieldName;
    public readonly string PropertyName;

    protected FieldEditor(string propertyName, string? propertyDescriptor, bool isSubObject,
        bool enterFieldDataOnCreate = false)
    {
        PropertyName = propertyName;
        _isSubObject = isSubObject;
        EnterFieldDataOnCreate = enterFieldDataOnCreate;
        FieldName = propertyDescriptor ?? PropertyName.SplitWithSpacesCamelParts();
    }

    public bool Enabled { get; set; } = true;

    public virtual void UpdateField(string? recordKey, object recordForUpdate)
    {
    }

    public void AddParameterEditMenuItem(CliMenuSet menuSet, ParametersEditor parametersEditor)
    {
        if (_isSubObject)
        {
            menuSet.AddMenuItem(new ParameterSubObjectFieldEditorCliMenuCommand(FieldName, this, parametersEditor));
        }
        else
        {
            menuSet.AddMenuItem(new ParameterFieldEditorCliMenuCommand(FieldName, this, parametersEditor));
        }
    }

    public void AddFieldEditMenuItem(CliMenuSet menuSet, ItemData recordForUpdate, Cruder cruder, string recordKey)
    {
        if (_isSubObject)
        {
            menuSet.AddMenuItem(new SubObjectFieldEditorCliMenuCommand(FieldName, this, recordForUpdate));
        }
        else
        {
            menuSet.AddMenuItem(new FieldEditorMenuCliMenuCommand(FieldName, this, recordForUpdate, cruder, recordKey));
        }
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
        PropertyInfo? currentRecordPropertyInfo = record.GetType().GetProperty(propertyName);
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
    protected readonly bool AutoUsageOfDefaultValue;
    protected readonly T? DefaultValue;

    protected FieldEditor(string propertyName, bool enterFieldDataOnCreate = false, T? defaultValue = default,
        bool autoUsageOfDefaultValue = false, string? propertyDescriptor = null, bool isSubObject = false) : base(
        propertyName, propertyDescriptor, isSubObject, enterFieldDataOnCreate)
    {
        DefaultValue = defaultValue;
        AutoUsageOfDefaultValue = autoUsageOfDefaultValue;
    }

    //აქ public საჭიროა CliParametersDataEdit.ConnectionStringParametersManager.Save მეთოდისათვის
    // ReSharper disable once MemberCanBeProtected.Global
    public void SetValue(object record, T? value)
    {
        PropertyInfo recordForUpdatePropertyInfo = record.GetType().GetProperty(PropertyName) ??
                                                   throw new DataInputException(
                                                       $"property {PropertyName} not found in record for update");
        recordForUpdatePropertyInfo.SetValue(record, value, null);
    }

    public override string GetValueStatus(object? record)
    {
        T? val = GetValueOrDefault(record);
        return val?.ToString() ?? string.Empty;
    }

    protected T? GetValueOrDefault(object? record)
    {
        T? val = AutoUsageOfDefaultValue && DefaultValue is not null
            ? GetValue(record, (T?)DefaultValue)
            : GetValue(record);

        return val;
    }

    //public override string GetValueStatus(object? record)
    //{
    //    var val = GetValue(record);
    //    return val is null ? string.Empty : val.ToString() ?? string.Empty;
    //}

    protected T? GetValue(object? record, T? defaultValue = default)
    {
        if (record == null)
        {
            return defaultValue;
        }

        PropertyInfo currentRecordPropertyInfo = record.GetType().GetProperty(PropertyName) ??
                                                 throw new DataInputException($"property {PropertyName} not found");

        object? toRet = currentRecordPropertyInfo.GetValue(record);
        if (toRet is null)
        {
            return defaultValue;
        }

        return (T)toRet;
    }

    public override void SetDefault(ItemData currentItem)
    {
        SetValue(currentItem, DefaultValue);
    }
}
