using System;
using Microsoft.EntityFrameworkCore;
using SystemToolsShared;

namespace DbContextAnalyzer;

public static class DbContextCreator
{
    public static T? Create<T>(string? connectionString) where T : DbContext
    {
        var optionsBuilder = new DbContextOptionsBuilder<T>();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            StShared.WriteErrorLine("connectionString is empty", true);
            return null;
        }
        optionsBuilder.UseSqlServer(connectionString);

        // Use Activator.CreateInstance with nullability check
        // ReSharper disable once using
        var context = Activator.CreateInstance(typeof(T), optionsBuilder.Options) as T;
        if (context != null) 
            return context;
        StShared.WriteErrorLine($"Failed to create an instance of {typeof(T).Name}", true);
        return null;

    }
}