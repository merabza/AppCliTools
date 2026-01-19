using System.Collections.Generic;

namespace AppCliTools.CodeTools;

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
}
