using System.Net;
using System.Text;
using Moq;
using NUnit.Framework;
using QueryLibrary.Models;
using QueryLibrary.Services;
using QueryLibraryTests.Model;

namespace QueryLibraryTests;

public class LibraryTests
{
    private const string HostName = "[AATracker.net] Community Server";
    private const string HostPort = "1716";
    private const string MapName = "Pipeline";
    private const string NumPlayers = "5";
    
    private QueryLibrary.IQueryLibrary _queryLibrary = null!;
    
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var mockResult = Encoding.UTF8.GetBytes($"hostname\0{HostName}\0hostport\0{HostPort}\0mapname\0{MapName}\0numplayers\0{NumPlayers}\0");
        var mockQueryRunner = new Mock<IQueryRunner>();
        mockQueryRunner.Setup(x => x.QueryServer(It.IsAny<IPEndPoint>(), It.IsAny<byte[]>())).ReturnsAsync(mockResult);
        
        _queryLibrary = new QueryLibrary.QueryLibrary(mockQueryRunner.Object, new QueryHelper(), new ResponseParser());
    }

    [Test]
    public async Task HostName_Parses_Correctly()
    {
        var (result, _) = await _queryLibrary.RunAsync<ServerStatus, Player>("127.0.0.1", 1716, QueryType.Status);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.hostname, Is.EqualTo(HostName));
        });
    }
    
    [Test]
    public async Task MapName_Parses_Correctly()
    {
        var (result, _) = await _queryLibrary.RunAsync<ServerStatus, Player>("127.0.0.1", 1716, QueryType.Status);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.mapname, Is.EqualTo(MapName));
        });
    }
    
    [Test]
    public async Task PlayerCount_Parses_Correctly()
    {
        const int expectedPlayerCount = 5;
        
        var (result, _) = await _queryLibrary.RunAsync<ServerStatus, Player>("127.0.0.1", 1716, QueryType.Status);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.PlayerCount, Is.EqualTo(expectedPlayerCount));
        });
    }

    [Test]
    public void Invalid_Host_Throws()
    {
        Assert.That(() => _queryLibrary.RunAsync<ServerStatus, Player>("invalid", 1716), Throws.TypeOf<ArgumentException>());
    }
}