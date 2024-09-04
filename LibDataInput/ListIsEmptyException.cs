namespace LibDataInput;

public sealed class ListIsEmptyException : DataInputException
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ListIsEmptyException(string message) : base(message)
    {
    }
}