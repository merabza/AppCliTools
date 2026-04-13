using System;
using System.Collections.Generic;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliTools.Services.RecentCommands.Models;

public sealed class RecentCommandsModel : IParameters
{
    public Dictionary<string, DateTime> Rc { get; set; } = [];

    public bool CheckBeforeSave()
    {
        return true;
    }
}
