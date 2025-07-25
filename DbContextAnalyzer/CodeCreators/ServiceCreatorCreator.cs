﻿using System;
using CodeTools;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;

namespace DbContextAnalyzer.CodeCreators;

public sealed class ServiceCreatorCreator : CodeCreator
{
    private readonly CreatorCreatorParameters _par;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ServiceCreatorCreator(ILogger logger, CreatorCreatorParameters par) : base(logger, par.SeedProjectPlacePath,
        "SeedDbServicesCreator.cs")
    {
        _par = par;
    }

    public override void CreateFileStructure()
    {
        var dataSeedingProjectName = $"{_par.DbProjectNamespace}DataSeeding"; //GeoModelDataSeeding
        var newDataSeedingProjectName = $"{_par.DbProjectNamespace}NewDataSeeding"; //GeoModelNewDataSeeding
        var projectDbProjectName = _par.DbProjectNamespace; // + "Db"; //GeoModelDb
        var repositoryClassName = $"{_par.ProjectPrefixShort}DataSeederRepository"; //GmDataSeederRepository
        var repositoryInterfaceName = $"I{repositoryClassName}"; //IGmDataSeederRepository
        var projectDbContextClassName = _par.ProjectDbContextClassName; // + "DbContext"; //GeoModelDbContext

        var block = new CodeBlock(string.Empty, new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            "using CarcassDataSeeding", "using CarcassDb", "using CarcassIdentity", "using CarcassMasterDataDom.Models",
            string.Empty, $"using {dataSeedingProjectName}", $"using {projectDbProjectName}",
            $"using {newDataSeedingProjectName}", string.Empty, "using Microsoft.AspNetCore.Identity",
            "using Microsoft.EntityFrameworkCore", "using Microsoft.Extensions.DependencyInjection",
            "using SystemToolsShared", string.Empty, $"namespace {_par.SeedProjectNamespace}", string.Empty,
            new CodeBlock("public sealed class SeedDbServicesCreator : ServicesCreator", string.Empty,
                "private readonly string _connectionString", string.Empty,
                "private readonly string _excludesRulesParametersFilePath", string.Empty,
                new OneLineComment(" ReSharper disable once ConvertToPrimaryConstructor"),
                new CodeBlock(
                    "public SeedDbServicesCreator(string? logFolder, string? logFileName, string appName, string connectionString, string excludesRulesParametersFilePath) : base(logFolder, logFileName, appName)",
                    "_connectionString = connectionString",
                    "_excludesRulesParametersFilePath = excludesRulesParametersFilePath"), string.Empty, new CodeBlock(
                    "protected override void ConfigureServices(IServiceCollection services)",
                    "base.ConfigureServices(services)", new OneLineComment("identity"),
                    "services.AddScoped<IUserStore<AppUser>, MyUserStore>()",
                    "services.AddScoped<IUserPasswordStore<AppUser>, MyUserStore>()",
                    "services.AddScoped<IUserEmailStore<AppUser>, MyUserStore>()",
                    "services.AddScoped<IUserRoleStore<AppUser>, MyUserStore>()",
                    "services.AddScoped<IRoleStore<AppRole>, MyUserStore>()", string.Empty,
                    "services.AddScoped<IIdentityRepository, IdentityRepository>()", string.Empty,
                    "services.AddScoped<ICarcassDataSeederRepository, CarcassDataSeederRepository>()", string.Empty,
                    $"services.AddScoped<{repositoryInterfaceName}, {repositoryClassName}>()", string.Empty,
                    "services.AddScoped<IDataFixRepository, DataFixRepository>()", string.Empty,
                    "services.AddDbContext<CarcassDbContext>(options => options.UseSqlServer(_connectionString))",
                    string.Empty,
                    $"services.AddDbContext<{projectDbContextClassName}>(options => options.UseSqlServer(_connectionString))",
                    string.Empty, """
                                  services.AddIdentity<AppUser, AppRole>(options =>
                                          {
                                            options.Password.RequiredLength = 3;
                                            options.Password.RequireNonAlphanumeric = false;
                                            options.Password.RequireLowercase = false;
                                            options.Password.RequireUppercase = false;
                                            options.Password.RequireDigit = false;
                                          }).AddDefaultTokenProviders()
                                  """)));
        CodeFile.AddRange(block.CodeItems);
        FinishAndSave();
    }
}