using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using QueryLibrary.Models;
using QueryLibrary.Resiliency;

namespace QueryLibrary;

public class QueryRunner(ILogger? logger = null, int portOffset = 0)
{
    public int TimeoutMs { get; set; } = 400;

    public async Task<(TStatusResult? Status, List<TPlayerResult>? Players)> RunAsync<TStatusResult, TPlayerResult>(string host, int port, QueryType queryType = QueryType.Both) 
        where TStatusResult : IServerStatus, new()
        where TPlayerResult : new()
    {
        var endPoint = await GetEndPointAsync(host, port);
        var request = await BuildRequestAsync(queryType);
        var response = await QueryServer(endPoint, request);
        return ResponseParser.Parse<TStatusResult, TPlayerResult>(response, queryType);
    }
    
    private async Task<byte[]> QueryServer(IPEndPoint endPoint, byte[] request)
    {
        return await ResilienceUtil.ExecuteSafelyAsync(async () =>
        {
            using var client = await CreateClientAndConnectAsync(endPoint);
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
        }, (ex, ts, count) => logger?.LogWarning("An error occurred on retry {count} - {message}. Retrying in: {timeout}", count, ex.Message, ts));
    }

    private static ValueTask<byte[]> BuildRequestAsync(QueryType queryType)
    {
        return queryType switch
        {
            QueryType.Both => new ValueTask<byte[]>(new byte[] { 0xfe, 0xfd, 0x00, 0x00, 0x00, 0x00, 0x01, 0xff, 0xff, 0x00 }),
            QueryType.Status => new ValueTask<byte[]>(new byte[] { 0xfe, 0xfd, 0x00, 0x00, 0x00, 0x00, 0x01, 0xff, 0x00, 0x00 }),
            _ => throw new ArgumentException("Invalid QueryType was provided")
        };
    }
    
    private ValueTask<IPEndPoint> GetEndPointAsync(string host, int port)
    {
        if (IPAddress.TryParse(host, out var address))
        {
            port += portOffset;
            return new ValueTask<IPEndPoint>(new IPEndPoint(address, port));
        }
        
        throw new ArgumentException($"Host: {address} is invalid format!");
    }

    private static ValueTask<UdpClient> CreateClientAndConnectAsync(IPEndPoint ep)
    {
        var client = new UdpClient();
        client.Client.Bind(new IPEndPoint(IPAddress.Any, 0)); // Receive on any host and port
        client.Connect(ep);
        return new ValueTask<UdpClient>(client);
    }
}