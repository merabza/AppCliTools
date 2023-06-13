using LibParameters;

namespace CliParameters;

public sealed class TextItemData : ItemData
{
    public TextItemData(string text)
    {
        Text = text;
    }

    private string Text { get; }

    public override string GetItemName()
    {
        return Text;
    }
}