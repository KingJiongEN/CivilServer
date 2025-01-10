
using GameServer.src;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();

        var host = builder.Build();
        host.Run();        
    }
}

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //读取配置
        ConfigLoader.Load();

        //启动
        GameWorld world = new GameWorld();
        if (world.Load())
        {
            ThreadPool.QueueUserWorkItem(async (obj) =>
            {
                await world.InitWorld();
            });

            world.BeforeRun(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await world.RunTick();
            }
            world.AfterRun();
        }
    }
}


