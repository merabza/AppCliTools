using System;
using System.Collections.Generic;
using LibParameters;

namespace CliTools;

public class RecentCommands : IParameters
{
    public Dictionary<string, DateTime> Rc = [];

    public bool CheckBeforeSave()
    {
        return true;
    }
}