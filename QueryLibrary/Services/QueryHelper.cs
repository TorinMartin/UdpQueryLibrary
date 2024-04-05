using System.Net;
using System.Net.Sockets;
using QueryLibrary.Models;

namespace QueryLibrary.Services;

public interface IQueryHelper
{
    public int PortOffset { get; init; }
    public Task<UdpClient> CreateClient(IPEndPoint endPoint);
    public ValueTask<byte[]> BuildRequestAsync(QueryType queryType);
    public ValueTask<IPEndPoint> GetEndPointAsync(string host, int port);
}

public class QueryHelper : IQueryHelper
{
    public int PortOffset { get; init; } = 1;
    
    public Task<UdpClient> CreateClient(IPEndPoint endPoint)
    {
        var client = new UdpClient();
        client.Client.Bind(new IPEndPoint(IPAddress.Any, 0)); // Receive on any host and port
        client.Connect(endPoint);
        return Task.FromResult(client);
    }

    public ValueTask<byte[]> BuildRequestAsync(QueryType queryType)
    {
        return queryType switch
        {
            QueryType.Both => new ValueTask<byte[]>(new byte[] { 0xfe, 0xfd, 0x00, 0x00, 0x00, 0x00, 0x01, 0xff, 0xff, 0x00 }),
            QueryType.Status => new ValueTask<byte[]>(new byte[] { 0xfe, 0xfd, 0x00, 0x00, 0x00, 0x00, 0x01, 0xff, 0x00, 0x00 }),
            _ => throw new ArgumentException("Invalid QueryType was provided")
        };
    }

    public ValueTask<IPEndPoint> GetEndPointAsync(string host, int port)
    {
        if (IPAddress.TryParse(host, out var address))
        {
            port += PortOffset;
            return new ValueTask<IPEndPoint>(new IPEndPoint(address, port));
        }
        
        throw new ArgumentException($"Host: {address} is invalid format!");
    }
}