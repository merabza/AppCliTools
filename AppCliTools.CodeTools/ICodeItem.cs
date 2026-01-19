namespace CodeTools;

public interface ICodeItem
{
    string Output(int indentLevel);
    string OutputCreator(int indentLevel, int additionalIndentLevel);
}