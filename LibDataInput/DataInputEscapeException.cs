namespace LibDataInput;
//ListIsEmptyException
public sealed class DataInputEscapeException : DataInputException
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public DataInputEscapeException(string message) : base(message)
    {
    }
}