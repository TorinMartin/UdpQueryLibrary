using QueryLibrary.Models;

namespace QueryLibrary.Services;

public interface IResponseParser
{
    public (TStatusResult Status, List<TPlayerResult>? Players) Parse<TStatusResult, TPlayerResult>(byte[] input, QueryType queryType) 
        where TStatusResult : IServerStatus, new() where TPlayerResult : new();
}

public class ResponseParser : IResponseParser
{
    public (TStatusResult Status, List<TPlayerResult>? Players) Parse<TStatusResult, TPlayerResult>(byte[] input, QueryType queryType)
        where TStatusResult : IServerStatus, new() where TPlayerResult : new()
    {
        return ResponseHelper.Parse<TStatusResult, TPlayerResult>(input, queryType);
    }
}