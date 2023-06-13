using System.Collections.Generic;

namespace CodeTools;

public sealed class CodeFile : CodeBlockBase, ICodeItem
{
    public CodeFile(string? fileName, params object[] codeList) : base(codeList)
    {
        FileName = fileName;
    }

    public CodeFile(List<ICodeItem> codeItems)
    {
        CodeItems = codeItems;
    }

    public string? FileName { get; set; }

    //public override string Output(int indentLevel)
    //{
    //  return base.Output(indentLevel);
    //}
}