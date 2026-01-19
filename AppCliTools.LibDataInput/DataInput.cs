using System;

namespace AppCliTools.LibDataInput;

public /*open*/ class DataInput
{
    public virtual bool DoInput()
    {
        return false;
    }

    protected static void ClearCurrentInput(int column)
    {
        int currentLine = Console.CursorTop;
        //int currentColumn = Console.CursorLeft;
        Console.SetCursorPosition(column, currentLine);
        Console.Write(new string(' ', Console.WindowWidth - column));
        Console.SetCursorPosition(column, currentLine);
    }
}
