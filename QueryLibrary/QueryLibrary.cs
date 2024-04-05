using Microsoft.Extensions.Logging;
using QueryLibrary.Models;
using QueryLibrary.Services;

namespace QueryLibrary;

public interface IQueryLibrary
{
    public Task<(TStatusResult? Status, List<TPlayerResult>? Players)> RunAsync<TStatusResult, TPlayerResult>(
        string host, int port, QueryType queryType = QueryType.Both)
        where TStatusResult : IServerStatus, new()
        where TPlayerResult : new();
}

public class QueryLibrary : IQueryLibrary
{
    private readonly IQueryRunner _queryRunner;
    private readonly IQueryHelper _queryHelper;
    private readonly IResponseParser _responseParser;
    private readonly ILogger? _logger;

    public QueryLibrary(IQueryRunner queryRunner, IQueryHelper queryHelper, IResponseParser responseParser, ILogger? logger = null)
    {
        _queryRunner = queryRunner;
        _queryHelper = queryHelper;
        _responseParser = responseParser;
        _logger = logger;
    }
    
    public async Task<(TStatusResult? Status, List<TPlayerResult>? Players)> RunAsync<TStatusResult, TPlayerResult>(
        string host, int port, QueryType queryType = QueryType.Both)
        where TStatusResult : IServerStatus, new()
        where TPlayerResult : new()
    {
        var endPoint = await _queryHelper.GetEndPointAsync(host, port);
        var request = await _queryHelper.BuildRequestAsync(queryType);
        var response = await _queryRunner.QueryServer(endPoint, request);
        
        return _responseParser.Parse<TStatusResult, TPlayerResult>(response, queryType);
    }
}