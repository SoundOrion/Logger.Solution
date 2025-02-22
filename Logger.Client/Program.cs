using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Http.BatchFormatters;
using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddHostedService<LoggingBackgroundService>();
                services.AddLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddSerilog();
                });
            })
            .UseSerilog((context, config) =>
            {
                config
                    .WriteTo.Console()
                    //.WriteTo.File("logs/client_log.txt", rollingInterval: RollingInterval.Day)
                    .WriteTo.Http("http://localhost:5031/api/logs",
                        queueLimitBytes: 1000,              // 最大1000件のログをキューに保持
                        period: TimeSpan.FromSeconds(5),    // 5秒ごとに送信
                        batchFormatter: new ArrayBatchFormatter());
            })
            .Build();

        await host.RunAsync();
    }
}

public class LoggingBackgroundService : BackgroundService
{
    private readonly ILogger<LoggingBackgroundService> _logger;

    public LoggingBackgroundService(ILogger<LoggingBackgroundService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("✅ クライアント: Serilog + HTTP のログ送信テスト");
            _logger.LogWarning("⚠️ クライアント: HTTP を使った警告ログ");
            _logger.LogError("❌ クライアント: エラーメッセージ");

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // 10秒ごとにログを送信
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Log.CloseAndFlush(); // アプリ終了時にログをフラッシュ
        return base.StopAsync(cancellationToken);
    }
}
