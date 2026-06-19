using System;

namespace AppCliTools.CliMenu;

//მენიუს ელემენტის სტატუსის ფერადი ნაწილი - ტექსტი თავის ფერთან ერთად
public sealed class StatusColorPart
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public StatusColorPart(string text, ConsoleColor color)
    {
        Text = text;
        Color = color;
    }

    public string Text { get; }
    public ConsoleColor Color { get; }
}
