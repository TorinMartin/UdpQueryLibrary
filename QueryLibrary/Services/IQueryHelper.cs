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