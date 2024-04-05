using System.Net;
using Microsoft.Extensions.Logging;
using QueryLibrary.Utils;

namespace QueryLibrary.Services;

public interface IQueryRunner
{
    public int TimeoutMs { get; set; }
    public Task<byte[]> QueryServer(IPEndPoint endPoint, byte[] request);
}

public class QueryRunner : IQueryRunner
{
    public int TimeoutMs { get; set; } = 400;
    private readonly IQueryHelper _queryHelper;
    private readonly ILogger? _logger;

    public QueryRunner(IQueryHelper queryHelper, ILogger? logger = null)
    {
        _queryHelper = queryHelper;
        _logger = logger;
    }
    
    public async Task<byte[]> QueryServer(IPEndPoint endPoint, byte[] request)
    {
        return await ResilienceUtil.ExecuteSafelyAsync(async () =>
        {
            using var client = await _queryHelper.CreateClient(endPoint);
            var send = client.SendAsync(request, request.Length);
            var receive = client.ReceiveAsync();
                
            using var sendCts = new CancellationTokenSource();
            await Task.WhenAny(send, Task.Delay(TimeoutMs, sendCts.Token));
            if (send.IsCompleted is false) throw new TimeoutException($"Send to {endPoint.Address} timed out");
            await sendCts.CancelAsync(); // Ensure we don't have a bunch of delay tasks running when send / receive are successful
                
            using var receiveCts = new CancellationTokenSource();
            await Task.WhenAny(receive, Task.Delay(TimeoutMs, receiveCts.Token));
            if (receive.IsCompletedSuccessfully)
            {
                await receiveCts.CancelAsync();
                return (await receive).Buffer.Skip(5).ToArray();
            }
                
            throw new TimeoutException($"Receive from {endPoint.Address} timed out");
        }, (ex, ts, count) => _logger?.LogWarning("An error occurred on retry {count} - {message}. Retrying in: {timeout}", count, ex.Message, ts));
    }
}