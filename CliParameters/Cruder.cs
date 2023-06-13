using System;
using System.Collections.Generic;
using System.Linq;
using CliMenu;
using CliParameters.FieldEditors;
using CliParameters.MenuCommands;
using LibDataInput;
using LibMenuInput;
using LibParameters;
using SystemToolsShared;

namespace CliParameters;

public /*open*/ class Cruder : IFieldEditors
{
    private readonly bool _canEditFieldsInSequence;
    private readonly bool _fieldNameFromItem;
    public readonly string CrudName;
    public readonly string CrudNamePlural;
    protected readonly List<FieldEditor> FieldEditors = new();

    protected Cruder(string crudName, string crudNamePlural, bool fieldNameFromItem = false,
        bool canEditFieldsInSequence = true)
    {
        CrudName = crudName;
        CrudNamePlural = crudNamePlural;
        _fieldNameFromItem = fieldNameFromItem;
        _canEditFieldsInSequence = canEditFieldsInSequence;
    }

    public int Count => GetKeys().Count;

    public void EnableFieldByName(string fieldName, bool enable = true)
    {
        var fieldEditor = FieldEditors.SingleOrDefault(s => s.PropertyName == fieldName);
        if (fieldEditor != null)
            fieldEditor.Enabled = enable;
    }

    protected virtual Dictionary<string, ItemData> GetCrudersDictionary()
    {
        return new Dictionary<string, ItemData>();
    }


    //public საჭიროა ApAgent პროექტისათვის
    // ReSharper disable once MemberCanBeProtected.Global
    public virtual bool ContainsRecordWithKey(string recordKey)
    {
        return false;
    }

    protected virtual void RemoveRecordWithKey(string recordKey)
    {
    }

    public virtual void UpdateRecordWithKey(string recordName, ItemData newRecord)
    {
    }

    protected virtual void AddRecordWithKey(string recordName, ItemData newRecord)
    {
    }

    private ItemData? InputRecordData(string recordName, ItemData? defaultItemData = null, bool createNew = false)
    {
        try
        {
            ItemData? currentItem;
            if (createNew)
            {
                currentItem = CreateNewItem(recordName, defaultItemData);
                foreach (var fieldUpdater in FieldEditors) fieldUpdater.SetDefault(currentItem);
            }
            else
            {
                currentItem = GetItemByName(recordName, false);
                if (currentItem is null)
                {
                    StShared.WriteErrorLine($"Record with name {recordName} not found", true);
                    return null;
                }
            }

            foreach (var fieldUpdater in FieldEditors.Where(fieldUpdater => fieldUpdater.Enabled))
            {
                fieldUpdater.UpdateField(recordName, currentItem);
                CheckFieldsEnables(currentItem, fieldUpdater.PropertyName);
            }

            if (CheckValidation(currentItem))
                return currentItem;

            return !Inputer.InputBool($"{recordName} is not valid {CrudName}, continue input data?", false, false)
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

    //recordName საჭიროა Crawler-ის პროექტისათვის
    protected virtual ItemData CreateNewItem(string recordName, ItemData? defaultItemData)
    {
        return new ItemData();
    }

    public virtual List<string> GetKeys()
    {
        var crudersDictionary = GetCrudersDictionary();
        return crudersDictionary.Keys.OrderBy(x => x).ToList();
    }

    protected virtual void CheckFieldsEnables(ItemData itemData, string? lastEditedFieldName = null)
    {
    }

    public virtual void FillDetailsSubMenu(CliMenuSet itemSubMenuSet, string itemName)
    {
        var item = GetItemByName(itemName);
        if (item == null)
            return;

        CheckFieldsEnables(item);

        var fieldName = "Record Name";
        itemSubMenuSet.AddMenuItem(new RecordNameEditorMenuCommand(fieldName, this, itemName));

        foreach (var fieldEditor in FieldEditors.Where(fieldUpdater => fieldUpdater.Enabled))
            fieldEditor.AddFieldEditMenuItem(itemSubMenuSet, item, this, itemName);
    }


    public CliMenuSet GetListMenu()
    {
        CliMenuSet cruderSubMenuSet = new(CrudNamePlural);

        NewItemCommand newItemCommand = new(this, CrudNamePlural, $"New {CrudName}");
        cruderSubMenuSet.AddMenuItem(newItemCommand);

        var itemDataDict = GetCrudersDictionary();

        foreach (var kvp in itemDataDict.OrderBy(o => o.Key))
            cruderSubMenuSet.AddMenuItem(new ItemSubMenuCommand(this, kvp.Key, CrudNamePlural), kvp.Key);

        FillListMenuAdditional(cruderSubMenuSet);

        var key = ConsoleKey.Escape.Value().ToLower();
        cruderSubMenuSet.AddMenuItem(key, "Exit to Main menu", new ExitToMainMenuCommand(null, null), key.Length);

        return cruderSubMenuSet;
    }

    protected virtual void FillListMenuAdditional(CliMenuSet cruderSubMenuSet)
    {
    }

    public bool DeleteRecord(string recordName)
    {
        if (!ContainsRecordWithKey(recordName))
        {
            StShared.WriteErrorLine($"{CrudName} with Name {recordName} does not exists and cannot be deleted. ",
                true);
            return false;
        }

        if (!Inputer.InputBool($"Are you sure, you wont to delete {recordName}", true, false))
            return false;

        RemoveRecordWithKey(recordName);
        Save($"{CrudName} with Name {recordName} deleted successfully.");

        return true;
    }

    public string? CreateNewRecord(string? currentStatus = null)
    {
        //ჩანაწერის შექმნის პროცესი დაიწყო
        Console.WriteLine($"Create new {CrudName} started");

        //ახალი ჩანაწერის სახელის შეტანა პროგრამაში
        TextDataInput nameInput = new($"New {CrudName} Name");
        if (!nameInput.DoInput())
            return null;
        var newRecordName = nameInput.Text;
        if (!CheckNewRecordNameValid(null, newRecordName))
            return null;

        if (string.IsNullOrWhiteSpace(newRecordName))
        {
            StShared.WriteErrorLine("New Record name is empty, cannot create new record", true);
            return null;
        }

        var defRecordWithStatus = GetDefRecordWithStatus(currentStatus);
        var newRecord = InputRecordData(newRecordName, defRecordWithStatus, true);

        if (newRecord == null)
            return null;

        AddRecordWithKey(newRecordName, newRecord);

        //პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
        Save($"Create new {CrudName} {newRecordName} Finished");

        //ყველაფერი კარგად დასრულდა
        return newRecordName;
    }


    public bool EditItemAllFieldsInSequence(string recordName)
    {
        if (!ContainsRecordWithKey(recordName))
        {
            StShared.WriteErrorLine($"{CrudName} with Name {recordName} does not exists and cannot be edited. ",
                true);
            return false;
        }

        //ჩანაწერის შეცვლის პროცესი დაიწყო
        Console.WriteLine($"Edit {CrudName} record started");

        //ამოცანის სახელის რედაქტირება
        TextDataInput nameInput = new($"change {CrudName} Name", recordName);
        if (!nameInput.DoInput())
            return false;
        var newRecordName = nameInput.Text;
        if (!CheckNewRecordNameValid(recordName, newRecordName))
            return false;

        var newRecord = InputRecordData(recordName);

        if (newRecord is null)
            return false;

        if (_fieldNameFromItem)
        {
            newRecordName = newRecord.GetItemName();
            if (!CheckNewRecordNameValid(recordName, newRecordName))
                return false;
        }

        if (newRecordName != recordName)
        {
            if (string.IsNullOrWhiteSpace(newRecordName))
            {
                StShared.WriteErrorLine("newRecordName is empty", true);
                return false;
            }

            RemoveRecordWithKey(recordName);
            AddRecordWithKey(newRecordName, newRecord);
        }
        else
        {
            UpdateRecordWithKey(recordName, newRecord);
        }

        //პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
        Save($"{CrudName} {newRecordName} Updated");

        //ყველაფერი კარგად დასრულდა
        return true;
    }

    private bool CheckNewRecordNameValid(string? recordName, string? newRecordName)
    {
        if (string.IsNullOrWhiteSpace(newRecordName))
            return false;

        if (newRecordName == recordName)
            return true;
        //გადავამოწმოთ ხომ არ არსებობს იგივე სახელით სხვა ჩანაწერი.
        if (!ContainsRecordWithKey(newRecordName))
            return true;
        StShared.WriteErrorLine(
            $"Another {CrudName} with Name {newRecordName} is already exists. cannot change {CrudName} name. ",
            true);
        return false;
    }

    public CliMenuSet GetItemMenu(string itemName, string? menuNamePrefix = null)
    {
        CliMenuSet itemSubMenuSet = new($"{menuNamePrefix ?? ""}{CrudName} => {itemName}:");

        DeleteCommand deleteCommand = new(this, itemName);
        itemSubMenuSet.AddMenuItem(deleteCommand, "Delete this record");

        if (_canEditFieldsInSequence)
        {
            EditItemAllFieldsInSequenceCommand editCommand = new(this, itemName);
            itemSubMenuSet.AddMenuItem(editCommand, "Edit All fields in sequence");
        }

        FillDetailsSubMenu(itemSubMenuSet, itemName);

        var key = ConsoleKey.Escape.Value().ToLower();
        itemSubMenuSet.AddMenuItem(key, $"Exit to {CrudNamePlural} menu", new ExitToMainMenuCommand(null, null),
            key.Length);

        return itemSubMenuSet;
    }

    protected virtual ItemData? GetDefRecordWithStatus(string? currentStatus)
    {
        return null;
    }

    public virtual void Save(string message)
    {
    }

    public ItemData? GetItemByName(string itemName, bool writeErrorIfNotExists = true)
    {
        var cruderDict = GetCrudersDictionary();

        if (cruderDict.ContainsKey(itemName))
            return cruderDict[itemName];

        if (writeErrorIfNotExists)
            StShared.WriteErrorLine($"{CrudName} with Name {itemName} does not exists. ", true);

        return null;
    }

    public string? GetNameWithPossibleNewName(string fieldName, string? currentName, string? currentStatus = null,
        bool useNone = false)
    {
        CliMenuSet listSet = new();

        if (useNone) listSet.AddMenuItem("-", "(None)", new CliMenuCommand(), 1);

        var id = 0;
        listSet.AddMenuItem(new MenuCommandWithStatus(currentStatus), $"New {fieldName}", id++);

        var keys = GetKeys();
        foreach (var listItem in keys)
            listSet.AddMenuItem(new ListItemMenuCommand(this, listItem), null, id++);

        var selectedId = MenuInputer.InputIdFromMenuList(fieldName.Pluralize(), listSet, currentName);


        if (useNone && selectedId == -1)
            return null;

        if (selectedId == 0)
        {
            var newName = CreateNewRecord(currentStatus);
            if (newName != null)
                return newName;
            throw new DataInputException($"{fieldName} does not created");
        }

        var index = selectedId - 1; // - oneMore;
        if (index >= 0 && index < keys.Count)
            return keys[index];

        throw new DataInputException("Selected invalid ID. ");
    }


    public virtual string? GetStatusFor(string name)
    {
        return null;
    }


    public bool ChangeRecordKey(string recordName, string newRecordName)
    {
        if (recordName == newRecordName)
            return true;
        if (ContainsRecordWithKey(newRecordName))
            return false;
        if (!ContainsRecordWithKey(recordName))
            return false;
        var itemData = GetItemByName(recordName);
        if (itemData is null)
            return false;
        RemoveRecordWithKey(recordName);
        AddRecordWithKey(newRecordName, itemData);
        return true;
    }
}