using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.LibDataInput;
using AppCliTools.LibMenuInput;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParameters.Cruders;

public /*open*/ class Cruder : IFieldEditors
{
    private readonly bool _canEditFieldsInSequence;

    private readonly bool _fieldKeyFromItem;
    public readonly string CrudName;
    public readonly string CrudNamePlural;
    protected readonly List<FieldEditor> FieldEditors = [];

    private CliMenuSet? _cruderSubMenuSet;
    private int _menuVersion;

    // ReSharper disable once ConvertToPrimaryConstructor
    protected Cruder(string crudName, string crudNamePlural, bool fieldKeyFromItem = false,
        bool canEditFieldsInSequence = true)
    {
        CrudName = crudName;
        CrudNamePlural = crudNamePlural;
        _fieldKeyFromItem = fieldKeyFromItem;
        _canEditFieldsInSequence = canEditFieldsInSequence;
    }

    public int Count => GetKeys().Count;

    public void EnableFieldByName(string fieldName, bool enable = true)
    {
        FieldEditor? fieldEditor = FieldEditors.SingleOrDefault(s => s.PropertyName == fieldName);
        fieldEditor?.Enabled = enable;
    }

    protected void EnableAllFieldButOne(string butOneFieldName, bool enable = true)
    {
        foreach (FieldEditor fieldEditor in FieldEditors)
        {
            fieldEditor.Enabled = fieldEditor.PropertyName == butOneFieldName || enable;
        }
    }

    protected void EnableOffAllFieldButList(List<string> butOneFieldName)
    {
        foreach (FieldEditor fieldEditor in FieldEditors)
        {
            fieldEditor.Enabled = butOneFieldName.Contains(fieldEditor.PropertyName);
        }
    }

    protected virtual Dictionary<string, ItemData> GetCrudersDictionary()
    {
        return [];
    }

    //public საჭიროა ApAgent პროექტისათვის
    // ReSharper disable once MemberCanBeProtected.Global
    public virtual bool ContainsRecordWithKey(string recordKey)
    {
        return false;
    }

    protected virtual ValueTask RemoveRecordWithKey(string recordKey, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask UpdateRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }

    protected virtual ValueTask AddRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }

    private async ValueTask<ItemData?> InputRecordData(string? recordKey = null, ItemData? defaultItemData = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ItemData? currentItem;
            if (recordKey is null)
            {
                currentItem = CreateNewItem(recordKey, defaultItemData);
                foreach (FieldEditor fieldUpdater in FieldEditors)
                {
                    fieldUpdater.SetDefault(currentItem);
                }
            }
            else
            {
                currentItem = GetItemByName(recordKey, false);
                if (currentItem is null)
                {
                    StShared.WriteErrorLine($"Record with name {recordKey} not found", true);
                    return null;
                }
            }

            foreach (FieldEditor fieldUpdater in FieldEditors.Where(fieldUpdater =>
                         fieldUpdater is { Enabled: true, EnterFieldDataOnCreate: true }))
            {
                await fieldUpdater.UpdateField(recordKey, currentItem, cancellationToken);
                CheckFieldsEnables(currentItem, fieldUpdater.PropertyName);
            }

            if (CheckValidation(currentItem))
            {
                return currentItem;
            }

            return !Inputer.InputBool($"{recordKey} is not valid {CrudName}, continue input data?", false, false)
                ? null
                : currentItem;
        }
        catch (DataInputEscapeException)
        {
            Console.WriteLine();
            Console.WriteLine("Escape... ");
            StShared.Pause();
            return null;
        }
        catch (Exception e)
        {
            StShared.WriteException(e, true);
            return null;
        }
    }

    public virtual bool CheckValidation(ItemData item)
    {
        return true;
    }

    //recordKey გამოყენებულია ქრაულერში
    // ReSharper disable once UnusedParameter.Global
    protected virtual ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return defaultItemData ?? new ItemData();
    }

    //public საჭიროა ApAgent.FieldEditors.ArchiverFieldEditor.UpdateField მეთოდისათვის
    // ReSharper disable once MemberCanBeProtected.Global
    public virtual List<string> GetKeys()
    {
        Dictionary<string, ItemData> crudersDictionary = GetCrudersDictionary();
        return [.. crudersDictionary.Keys.OrderBy(x => x)];
    }

    protected virtual void CheckFieldsEnables(ItemData itemData, string? lastEditedFieldName = null)
    {
    }

    //public საჭიროა SupportTools TemplateSubMenuCommand
    public virtual void FillDetailsSubMenu(CliMenuSet itemSubMenuSet, string itemName)
    {
        ItemData? item = GetItemByName(itemName);
        if (item == null)
        {
            return;
        }

        CheckFieldsEnables(item);

        if (!_fieldKeyFromItem)
        {
            const string fieldName = "Record Name";
            itemSubMenuSet.AddMenuItem(new RecordKeyEditorCliMenuCommand(fieldName, this, itemName));
        }

        foreach (FieldEditor fieldEditor in FieldEditors.Where(fieldUpdater => fieldUpdater.Enabled))
        {
            fieldEditor.AddFieldEditMenuItem(itemSubMenuSet, item, this, itemName);
        }
    }

    public CliMenuSet GetListMenu()
    {
        if (_cruderSubMenuSet is not null && _cruderSubMenuSet.MenuVersion == _menuVersion)
        {
            return _cruderSubMenuSet;
        }

        BeforeGetListMenu();

        _cruderSubMenuSet = new CliMenuSet(CrudNamePlural, _menuVersion);

        var newItemCommand = new NewItemCliMenuCommand(this, CrudNamePlural, $"New {CrudName}");
        _cruderSubMenuSet.AddMenuItem(newItemCommand);

        Dictionary<string, ItemData> itemDataDict = GetCrudersDictionary();

        foreach (KeyValuePair<string, ItemData> kvp in itemDataDict.OrderBy(o => o.Key))
        {
            _cruderSubMenuSet.AddMenuItem(new ItemSubMenuCliMenuCommand(this, kvp.Key, CrudNamePlural));
        }

        FillListMenuAdditional(_cruderSubMenuSet);

        // string key = ConsoleKey.Escape.Value().ToLower();
        string key = ConsoleKey.Escape.Value().ToUpperInvariant();
        _cruderSubMenuSet.AddMenuItem(key, new ExitToMainMenuCliMenuCommand("Exit to level up menu", null), key.Length);

        return _cruderSubMenuSet;
    }

    protected virtual void BeforeGetListMenu()
    {
    }

    protected virtual void FillListMenuAdditional(CliMenuSet cruderSubMenuSet)
    {
    }

    public async ValueTask<bool> DeleteRecord(string recordKey, CancellationToken cancellationToken = default)
    {
        if (!ContainsRecordWithKey(recordKey))
        {
            StShared.WriteErrorLine($"{CrudName} with Name {recordKey} does not exists and cannot be deleted. ", true);
            return false;
        }

        if (!Inputer.InputBool($"Are you sure, you wont to delete {recordKey}?", true, false))
        {
            return false;
        }

        await RemoveRecordWithKey(recordKey, cancellationToken);
        _menuVersion++;
        await Save($"{CrudName} with Name {recordKey} deleted successfully.", cancellationToken);

        return true;
    }

    public async ValueTask<string?> CreateNewRecord(string? currentStatus = null,
        CancellationToken cancellationToken = default)
    {
        //ჩანაწერის შექმნის პროცესი დაიწყო
        Console.WriteLine($"Create new {CrudName} started");

        string? newRecordKey = Guid.NewGuid().ToString();
        if (!_fieldKeyFromItem)
        {
            //ახალი ჩანაწერის სახელის შეტანა პროგრამაში
            newRecordKey = await InputNewRecordName(cancellationToken);
            if (string.IsNullOrWhiteSpace(newRecordKey))
            {
                return null;
            }

            if (!CheckNewRecordKeyValid(null, newRecordKey))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(newRecordKey))
            {
                StShared.WriteErrorLine("New Record name is empty, cannot create new record", true);
                return null;
            }
        }

        ItemData? defRecordWithStatus = GetDefRecordWithStatus(currentStatus);
        ItemData? newRecord = await InputRecordData(null, defRecordWithStatus, cancellationToken);

        if (newRecord is null)
        {
            StShared.WriteErrorLine($"New Record {CrudName} could not created", true);
            return null;
        }

        await AddRecordWithKey(newRecordKey, newRecord, cancellationToken);
        _menuVersion++;

        //პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
        await Save($"Create new {CrudName} {newRecordKey} Finished", cancellationToken);
        //ყველაფერი კარგად დასრულდა
        return newRecordKey;
    }

    protected virtual ValueTask<string?> InputNewRecordName(CancellationToken cancellationToken = default)
    {
        var nameInput = new TextDataInput($"New {CrudName} Name");
        return !nameInput.DoInput() ? ValueTask.FromResult<string?>(null) : ValueTask.FromResult(nameInput.Text);
    }

    public async ValueTask<bool> EditItemAllFieldsInSequence(string recordKey,
        CancellationToken cancellationToken = default)
    {
        if (!ContainsRecordWithKey(recordKey))
        {
            StShared.WriteErrorLine($"{CrudName} with Name {recordKey} does not exists and cannot be edited. ", true);
            return false;
        }

        //ჩანაწერის შეცვლის პროცესი დაიწყო
        Console.WriteLine($"Edit {CrudName} record started");
        string? newRecordKey = null;
        if (!_fieldKeyFromItem)
        {
            //ამოცანის სახელის რედაქტირება
            var nameInput = new TextDataInput($"change {CrudName} Name", recordKey);
            if (!nameInput.DoInput())
            {
                return false;
            }

            newRecordKey = nameInput.Text;
            if (!CheckNewRecordKeyValid(recordKey, newRecordKey))
            {
                return false;
            }
        }

        ItemData? newRecord = await InputRecordData(recordKey, null, cancellationToken);

        if (newRecord is null)
        {
            return false;
        }

        if (newRecordKey is not null && newRecordKey != recordKey)
        {
            if (string.IsNullOrWhiteSpace(newRecordKey))
            {
                StShared.WriteErrorLine("newRecordKey is empty", true);
                return false;
            }

            await RemoveRecordWithKey(recordKey, cancellationToken);
            await AddRecordWithKey(newRecordKey, newRecord, cancellationToken);
        }
        else
        {
            await UpdateRecordWithKey(recordKey, newRecord, cancellationToken);
        }

        _menuVersion++;

        //პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
        await Save($"{CrudName} {newRecordKey} Updated", cancellationToken);

        //ყველაფერი კარგად დასრულდა
        return true;
    }

    private bool CheckNewRecordKeyValid(string? recordKey, string? newRecordKey)
    {
        if (string.IsNullOrWhiteSpace(newRecordKey))
        {
            return false;
        }

        if (newRecordKey == recordKey)
        {
            return true;
        }

        //გადავამოწმოთ ხომ არ არსებობს იგივე სახელით სხვა ჩანაწერი.
        if (!ContainsRecordWithKey(newRecordKey))
        {
            return true;
        }

        StShared.WriteErrorLine(
            $"Another {CrudName} with Name {newRecordKey} is already exists. cannot change {CrudName} name. ", true);
        return false;
    }

    public CliMenuSet GetItemMenu(string itemName)
    {
        string substituteName = itemName;
        ItemData? item = GetItemByName(itemName);
        if (item is not null)
        {
            substituteName = item.GetItemKey() ?? substituteName;
        }

        var itemSubMenuSet = new CliMenuSet(substituteName);

        var deleteCommand = new DeleteCruderRecordCliMenuCommand(this, itemName);
        itemSubMenuSet.AddMenuItem(deleteCommand);

        if (_canEditFieldsInSequence)
        {
            var editCommand = new EditItemAllFieldsInSequenceCliMenuCommand(this, itemName);
            itemSubMenuSet.AddMenuItem(editCommand);
        }

        FillDetailsSubMenu(itemSubMenuSet, itemName);

        // string key = ConsoleKey.Escape.Value().ToLower();
        string key = ConsoleKey.Escape.Value().ToUpperInvariant();
        itemSubMenuSet.AddMenuItem(key, new ExitToMainMenuCliMenuCommand($"Exit to {CrudNamePlural} menu", null),
            key.Length);

        return itemSubMenuSet;
    }

    protected virtual ItemData? GetDefRecordWithStatus(string? currentStatus)
    {
        return null;
    }

    public virtual ValueTask Save(string message, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }

    public ItemData? GetItemByName(string itemName, bool writeErrorIfNotExists = true)
    {
        Dictionary<string, ItemData> cruderDict = GetCrudersDictionary();

        if (cruderDict.TryGetValue(itemName, out ItemData? itemData))
        {
            return itemData;
        }

        if (writeErrorIfNotExists)
        {
            StShared.WriteErrorLine($"{CrudName} with Name {itemName} is not exists. ", true);
        }

        return null;
    }

    public async ValueTask<string?> GetNameWithPossibleNewName(string fieldName, string? currentName,
        string? currentStatus = null, bool useNone = false, CancellationToken cancellationToken = default)
    {
        var listSet = new CliMenuSet();

        if (useNone)
        {
            listSet.AddMenuItem("-", new CliMenuCommand("(None)"), 1);
        }

        int id = 0;
        listSet.AddMenuItem(new MenuCommandWithStatusCliMenuCommand($"New {fieldName}"), id++);

        List<string> keys = GetKeys();
        foreach (string listItem in keys)
        {
            listSet.AddMenuItem(new ListItemCliMenuCommand(this, listItem), id++);
        }

        int selectedId = MenuInputer.InputIdFromMenuList(fieldName.Pluralize(), listSet, currentName);

        if (useNone && selectedId == -1)
        {
            return null;
        }

        if (selectedId == 0)
        {
            string? newName = await CreateNewRecord(currentStatus, cancellationToken);
            return newName ?? throw new DataInputException($"{fieldName} does not created");
        }

        int index = selectedId - 1; // - oneMore;
        if (index >= 0 && index < keys.Count)
        {
            return keys[index];
        }

        throw new DataInputException("Selected invalid ID. ");
    }

    public virtual string? GetStatusFor(string name)
    {
        return null;
    }

    public async ValueTask<bool> ChangeRecordKey(string recordKey, string newRecordKey,
        CancellationToken cancellationToken = default)
    {
        if (recordKey == newRecordKey)
        {
            return true;
        }

        if (ContainsRecordWithKey(newRecordKey))
        {
            return false;
        }

        if (!ContainsRecordWithKey(recordKey))
        {
            return false;
        }

        ItemData? itemData = GetItemByName(recordKey);
        if (itemData is null)
        {
            return false;
        }

        await RemoveRecordWithKey(recordKey, cancellationToken);
        await AddRecordWithKey(newRecordKey, itemData, cancellationToken);
        _menuVersion++;
        return true;
    }
}
