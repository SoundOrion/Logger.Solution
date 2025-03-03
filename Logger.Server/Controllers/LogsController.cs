﻿using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[Route("api/logs")]
public class LogsController : ControllerBase
{
    private readonly ILogger<LogsController> _logger;

    public LogsController(ILogger<LogsController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveLogs([FromBody] JsonElement logs)
    {
        // 受信データを文字列に変換（デバッグ用）
        var receivedLogs = JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });

        // 必要に応じてデータベースやファイルに保存する処理を追加
        _logger.LogInformation("Received logs: {receivedLogs}", receivedLogs); // Loki に転送など

        return Ok(new { message = "Logs received successfully" });
    }
}

