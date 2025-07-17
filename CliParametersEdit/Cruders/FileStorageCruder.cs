using System;
using System.Collections.Generic;
using CliParameters;
using CliParameters.FieldEditors;
using CliParametersEdit.FieldEditors;
using FileManagersMain;
using LibFileParameters.Interfaces;
using LibFileParameters.Models;
using LibParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace CliParametersEdit.Cruders;

public sealed class FileStorageCruder : ParCruder<FileStorageData>
{
    private readonly ILogger _logger;

    public FileStorageCruder(ILogger logger, IParametersManager parametersManager,
        Dictionary<string, FileStorageData> currentValuesDictionary) : base(parametersManager, currentValuesDictionary,
        "File Storage", "File Storages")
    {
        _logger = logger;
        FieldEditors.Add(new FileStoragePathFieldEditor(nameof(FileStorageData.FileStoragePath), true));
        FieldEditors.Add(new TextFieldEditor(nameof(FileStorageData.UserName)));
        FieldEditors.Add(new TextFieldEditor(nameof(FileStorageData.Password), null, false,
            ParametersEditor.PasswordChar));
        FieldEditors.Add(new IntFieldEditor(nameof(FileStorageData.FileNameMaxLength), 255));
        FieldEditors.Add(new IntFieldEditor(nameof(FileStorageData.FileSizeSplitPositionInRow), 4));
        FieldEditors.Add(new IntFieldEditor(nameof(FileStorageData.FtpSiteLsFileOffset), 71));
    }

    public static FileStorageCruder Create(ILogger logger, IParametersManager parametersManager)
    {
        var parameters = (IParametersWithFileStorages)parametersManager.Parameters;
        return new FileStorageCruder(logger, parametersManager, parameters.FileStorages);
    }

    //protected override Dictionary<string, ItemData> GetCrudersDictionary()
    //{
    //    var parameters = (IParametersWithFileStorages)ParametersManager.Parameters;
    //    return parameters.FileStorages.ToDictionary(p => p.Key, ItemData (p) => p.Value);
    //}

    //public override bool ContainsRecordWithKey(string recordKey)
    //{
    //    var parameters = (IParametersWithFileStorages)ParametersManager.Parameters;
    //    var fileStorages = parameters.FileStorages;
    //    return fileStorages.ContainsKey(recordKey);
    //}

    //public override void UpdateRecordWithKey(string recordKey, ItemData newRecord)
    //{
    //    var newFileStorage = (FileStorageData)newRecord;
    //    var parameters = (IParametersWithFileStorages)ParametersManager.Parameters;
    //    if (parameters is null)
    //        throw new Exception("parameters is null, cannot Update Record");
    //    parameters.FileStorages[recordKey] = newFileStorage;
    //}

    //protected override void AddRecordWithKey(string recordKey, ItemData newRecord)
    //{
    //    var newFileStorage = (FileStorageData)newRecord;
    //    var parameters = (IParametersWithFileStorages)ParametersManager.Parameters;
    //    if (parameters is null)
    //        throw new Exception("parameters is null, cannot Add Record");
    //    parameters.FileStorages.Add(recordKey, newFileStorage);
    //}

    //protected override void RemoveRecordWithKey(string recordKey)
    //{
    //    var parameters = (IParametersWithFileStorages)ParametersManager.Parameters;
    //    var fileStorages = parameters.FileStorages;
    //    fileStorages.Remove(recordKey);
    //}

    public override bool CheckValidation(ItemData item)
    {
        try
        {
            if (item is not FileStorageData fileStorageData)
                return false;

            if (fileStorageData.FileStoragePath is null)
                return false;

            if (FileStat.IsFileSchema(fileStorageData.FileStoragePath))
                return true;

            var rfm = RemoteFileManager.Create(fileStorageData, true, _logger, null);

            if (rfm == null)
                return false;

            Console.WriteLine("Try connect to file server...");

            if (!rfm.CheckConnection())
                return false;

            Console.WriteLine("Connected successfully");

            return true;
        }
        catch (Exception e)
        {
            StShared.WriteException(e, true);
            return false;
        }
    }

    protected override void CheckFieldsEnables(ItemData itemData, string? lastEditedFieldName = null)
    {
        if (itemData is not FileStorageData fileStorageData)
            return;
        var isFileSchema = fileStorageData.IsFileSchema();
        var enableUserNamePassword = isFileSchema is not null && !isFileSchema.Value;
        EnableFieldByName(nameof(FileStorageData.UserName), enableUserNamePassword);
        EnableFieldByName(nameof(FileStorageData.Password), enableUserNamePassword);
        var isFtp = fileStorageData.IsFtp();
        var enableFtpProperties = isFtp is not null && isFtp.Value;
        EnableFieldByName(nameof(FileStorageData.FileSizeSplitPositionInRow), enableFtpProperties);
        EnableFieldByName(nameof(FileStorageData.FtpSiteLsFileOffset), enableFtpProperties);
    }

    //protected override ItemData GetDefRecordWithStatus(string? currentStatus)
    //{
    //    return new FileStorageData { FileStoragePath = currentStatus };
    //}

    public override string? GetStatusFor(string name)
    {
        var fileStorage = (FileStorageData?)GetItemByName(name);
        return fileStorage?.FileStoragePath;
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        if (defaultItemData is FileStorageData defFileStorageData)
            return new FileStorageData { FileStoragePath = defFileStorageData.FileStoragePath };
        return new FileStorageData();
    }
}