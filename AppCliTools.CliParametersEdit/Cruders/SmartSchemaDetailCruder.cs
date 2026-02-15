using System.Collections.Generic;
using System.Linq;
using AppCliTools.CliParameters.Cruders;
using AppCliTools.CliParameters.FieldEditors;
using ParametersManagement.LibFileParameters.Models;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersEdit.Cruders;

public sealed class SmartSchemaDetailCruder : Cruder
{
    private readonly List<SmartSchemaDetail> _currentValuesList;

    public SmartSchemaDetailCruder(List<SmartSchemaDetail> currentValuesList) : base("Smart Schema Detail",
        "Smart Schema Details")
    {
        _currentValuesList = currentValuesList;

        FieldEditors.Add(new EnumFieldEditor<EPeriodType>(nameof(SmartSchemaDetail.PeriodType), EPeriodType.Day));
        FieldEditors.Add(new IntFieldEditor(nameof(SmartSchemaDetail.PreserveCount)));
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        return _currentValuesList.ToDictionary(p => p.PeriodType.ToString(), ItemData (p) => p);
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        return _currentValuesList.Select(s => s.PeriodType.ToString()).Contains(recordKey);
    }

    protected override void RemoveRecordWithKey(string recordKey)
    {
        SmartSchemaDetail? rbk = _currentValuesList.SingleOrDefault(w => w.PeriodType.ToString() == recordKey);
        if (rbk != null)
        {
            _currentValuesList.Remove(rbk);
        }
    }

    public override void UpdateRecordWithKey(string recordKey, ItemData newRecord)
    {
        if (newRecord is not SmartSchemaDetail newSmartSchemaDetail)
        {
            return;
        }

        if (recordKey != newSmartSchemaDetail.PeriodType.ToString())
        {
            return;
        }

        SmartSchemaDetail? rbk = _currentValuesList.SingleOrDefault(w => w.PeriodType.ToString() == recordKey);
        rbk?.PreserveCount = newSmartSchemaDetail.PreserveCount;
    }

    protected override void AddRecordWithKey(string recordKey, ItemData newRecord)
    {
        if (newRecord is SmartSchemaDetail sid)
        {
            _currentValuesList.Add(sid);
        }
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new SmartSchemaDetail();
    }

    public override List<string> GetKeys()
    {
        return _currentValuesList.Select(s => s.PeriodType.ToString()).OrderBy(x => x).ToList();
    }
}
