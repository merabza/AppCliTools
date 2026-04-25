using System;
using AppCliTools.CliTools.Services.RecentCommands;
using AppCliTools.CliTools.Services.RecentCommands.Models;
using Microsoft.Extensions.DependencyInjection;

namespace AppCliTools.CliTools.DependencyInjection;

public static class RecentCommandsServicesExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddRecentCommandsService(this IServiceCollection services,
        Action<RecentCommandOptions> setupAction)
    {
        services.AddSingleton<IRecentCommandsService, RecentCommandsService>();
        services.Configure(setupAction);
        return services;
    }
}
