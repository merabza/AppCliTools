using System;

namespace LibDataInput;

public /*open*/ class DataInputException : Exception
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public DataInputException(string message) : base(message)
    {
    }
}