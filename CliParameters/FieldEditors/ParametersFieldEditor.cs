﻿using CliMenu;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParameters.FieldEditors;

public abstract class ParametersFieldEditor<TModel, TEditor> : FieldEditor<TModel>
    where TModel : ParametersWithStatus, new() where TEditor : ParametersEditor
{
    protected readonly ILogger Logger;
    protected readonly IParametersManager ParametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    protected ParametersFieldEditor(string propertyName, ILogger logger, IParametersManager parametersManager,
        bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate, null, false, null, true)
    {
        Logger = logger;
        ParametersManager = parametersManager;
    }

    public override string GetValueStatus(object? record)
    {
        var val = GetValue(record);
        return val == null ? "(empty)" : val.GetStatus();
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        var currentValue = GetValue(record);
        if (currentValue == null)
        {
            currentValue = new TModel();
            SetValue(record, currentValue);
        }

        var parametersEditor = CreateEditor(record, currentValue);
        parametersEditor.CheckFieldsEnables(currentValue);
        return parametersEditor.GetParametersMainMenu();
    }

    protected abstract TEditor CreateEditor(object record, TModel currentValue);
}