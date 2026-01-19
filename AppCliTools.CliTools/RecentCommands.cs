using System;
using System.Collections.Generic;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliTools;

public sealed class RecentCommands : IParameters
{
    public Dictionary<string, DateTime> Rc { get; set; } = [];

    public bool CheckBeforeSave()
    {
        return true;
    }
}
