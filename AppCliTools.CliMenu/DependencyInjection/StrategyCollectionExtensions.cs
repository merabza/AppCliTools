using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace AppCliTools.CliMenu.DependencyInjection;

public static class StrategyCollectionExtensions
{
    public static IServiceCollection AddTransientAllStrategies<TStrategy>(this IServiceCollection services,
        params Assembly[] assemblies)
    {
        foreach (Assembly assembly in assemblies)
        {
            foreach (Type type in assembly.ExportedTypes.Where(x =>
                         typeof(TStrategy).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false }))
            {
                services.AddTransient(typeof(TStrategy), type);
            }
        }

        return services;
    }
}
