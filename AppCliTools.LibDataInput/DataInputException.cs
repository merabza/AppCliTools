using System;

namespace AppCliTools.LibDataInput;

public /*open*/ class DataInputException : Exception
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public DataInputException(string message) : base(message)
    {
    }
}
