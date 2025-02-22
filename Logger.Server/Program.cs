using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Serilog 設定（Loki 送信用）
builder.Host.UseSerilog((context, services, config) =>
{
    config
        .WriteTo.Console()
        //.WriteTo.File("logs/server_log.txt", rollingInterval: RollingInterval.Day) // ローカル保存
        .WriteTo.GrafanaLoki("http://localhost:3100",
            new List<LokiLabel>
            {
                new LokiLabel { Key = "app", Value = "log-server" },
                new LokiLabel { Key = "env", Value = "production" }
            },
            batchPostingLimit: 50,
            queueLimit: 1000,
            textFormatter: new JsonFormatter()); // JSON 形式で送信
});

// DI 設定
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
