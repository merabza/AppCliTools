using System;

namespace AppCliTools.LibDataInput;

[Flags]
public enum EDialogResult
{
    Yes = 1,
    No = 2,
    All = 4,
    NoOne = 8
}
