using System.Net;
using NUnit.Framework;
using QueryLibrary.Services;
using QueryLibraryTests.Model;

namespace QueryLibraryTests;

public class UnitTests
{
    private IQueryHelper _helper = null!;
    private IQueryRunner _queryRunner = null!;
    private QueryLibrary.IQueryLibrary _queryLibrary = null!;
    
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _helper = new QueryHelper();
        _queryRunner = new QueryRunner(_helper);
        _queryLibrary = new QueryLibrary.QueryLibrary(_queryRunner, _helper);
    }

    [Test]
    public void Invalid_Host_Throws()
    {
        Assert.That(() => _queryLibrary.RunAsync<ServerStatus, Player>("invalid", 1716), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Request_Times_Out()
    {
        _queryRunner.TimeoutMs = 1;
        Assert.That(() => _queryLibrary.RunAsync<ServerStatus, Player>("127.0.0.1", 1716), Throws.TypeOf<TimeoutException>());
    }

    [Test]
    public async Task GetEndPointAsync_ReturnsValidEndPoint()
    {
        var validHost = "127.0.0.1";
        var port = 1716;
        var expectedPort = 1716 + _helper.PortOffset;
        
        var endPoint = await _helper.GetEndPointAsync(validHost, port);
        
        Assert.Multiple(() =>
        {
            Assert.That(endPoint, Is.Not.Null);
            Assert.That(endPoint.Port, Is.EqualTo(expectedPort));
            Assert.That(IPAddress.Parse(validHost), Is.EqualTo(endPoint.Address));
        });
    }

    [Test]
    public void GetEndPointAsync_ThrowsOnInvalidHost()
    {
        Assert.That(() => _helper.GetEndPointAsync("invalid", 1716), Throws.TypeOf<ArgumentException>());
    }
}