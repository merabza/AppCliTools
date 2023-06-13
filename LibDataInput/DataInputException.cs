using System;

namespace LibDataInput;

public /*open*/ class DataInputException : Exception
{
    public DataInputException(string message) : base(message)
    {
    }
}