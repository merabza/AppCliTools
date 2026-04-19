//using ParametersManagement.LibParameters;

//namespace AppCliTools.CliParameters;

//public class ArgumentsParserResult<T> where T : class, IParameters, new()
//{
//    public ArgumentsParserResult(T par, string parametersFileName, EParseResult parseResult)
//    {
//        Par = par;
//        ParametersFileName = parametersFileName;
//        ParseResult = parseResult;
//    }

//    public T Par { get; set; }
//    public string ParametersFileName { get; set; }
//    public EParseResult ParseResult { get; set; }

//    public static ArgumentsParserResult<T> Create(T? par, string? parametersFileName, EParseResult parseResult)
//    {
//        return new ArgumentsParserResult<T>(par, parametersFileName, parseResult);
//    }

//}
