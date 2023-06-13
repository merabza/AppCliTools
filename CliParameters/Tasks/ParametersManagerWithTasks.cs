//using System;
//using System.Collections.Generic;
//using Microsoft.Extensions.Logging;

//namespace CliParameters.Tasks;

//public sealed class ParametersManagerWithTasks<TE> : ParametersManager  where TE : Enum
//{

//    public ParametersManagerWithTasks(string parametersFileName, IParametersWithTasks parameters, string encKey = null) :
//        base(parametersFileName, parameters, encKey)
//    {

//    }

//    public virtual IEnumerable<TE> GetStandAloneTools()
//    {
//        return null;
//    }

//    public virtual IParameters? CreateNewTaskParameters(TE tool)
//    {
//        return null;
//    }

//    public virtual IToolCommand CreateToolCommand(ILogger logger, TE tool, string taskName)
//    {
//        return null;
//    }

//}

