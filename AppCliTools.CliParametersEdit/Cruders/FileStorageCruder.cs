using System;
using System.Collections.Generic;
using AppCliTools.CliParameters;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliParametersEdit.FieldEditors;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibFileParameters.Interfaces;
using ParametersManagement.LibFileParameters.Models;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;
using ToolsManagement.FileManagersMain;

namespace AppCliTools.CliParametersEdit.Cruders;

public sealed class FileStorageCruder : ParCruder<FileStorageData>
{
    private readonly ILogger _logger;

    //public კონსტრუქტორი საჭიროა. გამოიყენება რეფლექსიით DictionaryFieldEditor-ში
    // ReSharper disable once MemberCanBePrivate.Global
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

    public override bool CheckValidation(ItemData item)
    {
        try
        {
            if (item is not FileStorageData fileStorageData)
            {
                return false;
            }

            if (fileStorageData.FileStoragePath is null)
            {
                return false;
            }

            if (FileStat.IsFileSchema(fileStorageData.FileStoragePath))
            {
                return true;
            }

            var rfm = RemoteFileManager.Create(fileStorageData, true, _logger, null);

            if (rfm == null)
            {
                return false;
            }

            Console.WriteLine("Try connect to file server...");

            if (!rfm.CheckConnection())
            {
                return false;
            }

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
        {
            return;
        }

        bool? isFileSchema = fileStorageData.IsFileSchema();
        bool enableUserNamePassword = isFileSchema is not null && !isFileSchema.Value;
        EnableFieldByName(nameof(FileStorageData.UserName), enableUserNamePassword);
        EnableFieldByName(nameof(FileStorageData.Password), enableUserNamePassword);
        bool? isFtp = fileStorageData.IsFtp();
        bool enableFtpProperties = isFtp is not null && isFtp.Value;
        EnableFieldByName(nameof(FileStorageData.FileSizeSplitPositionInRow), enableFtpProperties);
        EnableFieldByName(nameof(FileStorageData.FtpSiteLsFileOffset), enableFtpProperties);
    }

    public override string? GetStatusFor(string name)
    {
        var fileStorage = (FileStorageData?)GetItemByName(name);
        return fileStorage?.FileStoragePath;
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        if (defaultItemData is FileStorageData defFileStorageData)
        {
            return new FileStorageData { FileStoragePath = defFileStorageData.FileStoragePath };
        }

        return new FileStorageData();
    }
}
