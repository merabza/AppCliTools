using System;
using CliParameters;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using CliShared;
using Serilog.Events;
using LibSeedCodeCreator;

namespace SeedCodeCreator
{
    public class Program
    {
        //როცა საჭიროა შენახული მონაცემების დაშიფვრა, გამოიყენე ეს
        //private static readonly string Key = "5611219a-e3a4-4986-88a8-ca48ea6e6c88" + Environment.MachineName.Capitalize();
        //როცა არ არის საჭირო შენახული მონაცემების დაშიფვრა, გამოიყენე ეს
        private static readonly string Key = null;

        public static int Main(string[] args)
        {
            ILogger<Program> logger = null;

            try
            {
                Console.WriteLine(
                    "CreateGeoModelSeederCode Creates Code for apps GetJsonFromGeoModelDb and SeedGeoModelDb");

                IArgumentsParser argParser =
                    new ArgumentsParser<CreatorCreatorParameters>(args, "SeedCodeCreator", Key);

                switch (argParser.Analysis())
                {
                    case EParseResult.Ok: break;
                    case EParseResult.Usage: return 1;
                    case EParseResult.Error: return 1;
                    default: throw new ArgumentOutOfRangeException();
                }

                CreatorCreatorParameters par = (CreatorCreatorParameters)argParser.Par;
                string parametersFileName = argParser.ParametersFileName;
                ServicesCreator servicesCreator =
                    new ServicesCreator(par.LogFolder, par.LogFileName, "SeedCodeCreator");
                ServiceProvider serviceProvider = servicesCreator.CreateServiceProvider(LogEventLevel.Information);

                if (serviceProvider == null)
                {
                    Console.WriteLine("Logger not created");
                    return 8;
                }

                logger = serviceProvider.GetService<ILogger<Program>>();

                CreatorCreator creatorCreator =
                    new CreatorCreator(logger, new ParametersManager(parametersFileName, par));

                return creatorCreator.Run() ? 0 : 1;
            }
            catch (Exception e)
            {
                if (logger != null)
                    logger.LogError(e, "error");
                else
                    Console.WriteLine(e);
                BeforeReturn();
                return 7;
            }
        }

        private static void BeforeReturn()
        {
            Log.CloseAndFlush();
            Console.WriteLine("Press any kay ...");
            Console.ReadKey();
        }
    }
}