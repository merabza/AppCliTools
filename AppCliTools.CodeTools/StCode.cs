namespace AppCliTools.CodeTools;

public static class StCode
{
    public static string Quotas(this string code)
    {
        return $"\"{code.Replace("\"", "\\\"")}\"";
    }
}
