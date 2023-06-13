namespace LibDataInput;

public sealed class DataInputEscapeException : DataInputException
{
    public DataInputEscapeException(string message) : base(message)
    {
    }
}