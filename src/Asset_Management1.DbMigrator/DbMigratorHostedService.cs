﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Asset_Management1.Data;
using Serilog;
using Volo.Abp;

namespace Asset_Management1.DbMigrator;

public class DbMigratorHostedService : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IConfiguration _configuration;

    public DbMigratorHostedService(IHostApplicationLifetime hostApplicationLifetime, IConfiguration configuration)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var application = await AbpApplicationFactory.CreateAsync<Asset_Management1DbMigratorModule>(options =>
        {
           options.Services.ReplaceConfiguration(_configuration);
           options.UseAutofac();
           options.Services.AddLogging(c => c.AddSerilog());
        }))
        {
            await application.InitializeAsync();

            await application
                .ServiceProvider
                .GetRequiredService<Asset_Management1DbMigrationService>()
                .MigrateAsync();

            await application.ShutdownAsync();

            _hostApplicationLifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
