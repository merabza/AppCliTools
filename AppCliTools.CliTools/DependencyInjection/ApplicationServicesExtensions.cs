using System;
using AppCliTools.CliTools.App;
using Microsoft.Extensions.DependencyInjection;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliTools.DependencyInjection;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services,
        Action<ApplicationOptions> setupAction)
    {
        services.AddSingleton<IApplication, Application>();
        services.Configure(setupAction);
        return services;
    }
}
