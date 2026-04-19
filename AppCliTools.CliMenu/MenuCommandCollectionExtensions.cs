//using System;
//using System.Linq;
//using System.Reflection;
//using Microsoft.Extensions.DependencyInjection;

//namespace AppCliTools.CliMenu;

//public static class MenuCommandCollectionExtensions
//{
//    public static void AddTransientAllMenuCommandFactoryStrategies(this IServiceCollection services,
//        params Assembly[] assemblies)
//    {
//        foreach (Assembly assembly in assemblies)
//        {
//            foreach (Type type in assembly.ExportedTypes.Where(x =>
//                         typeof(IMenuCommandFactoryStrategy).IsAssignableFrom(x) &&
//                         x is { IsInterface: false, IsAbstract: false }))
//            {
//                services.AddTransient(typeof(IMenuCommandFactoryStrategy), type);
//            }
//        }
//    }

//    public static void AddTransientAllMenuCommandsListFactoryStrategies(this IServiceCollection services,
//        params Assembly[] assemblies)
//    {
//        foreach (Assembly assembly in assemblies)
//        {
//            foreach (Type type in assembly.ExportedTypes.Where(x =>
//                         typeof(IMenuCommandListFactoryStrategy).IsAssignableFrom(x) &&
//                         x is { IsInterface: false, IsAbstract: false }))
//            {
//                services.AddTransient(typeof(IMenuCommandListFactoryStrategy), type);
//            }
//        }
//    }
//}
