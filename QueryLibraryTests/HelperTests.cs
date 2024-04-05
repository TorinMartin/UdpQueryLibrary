using System.Net;
using NUnit.Framework;
using QueryLibrary.Services;

namespace QueryLibraryTests;

public class HelperTests
{
    private IQueryHelper _helper = null!;
    
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _helper = new QueryHelper();
    }
    
    [Test]
    public async Task GetEndPointAsync_ReturnsValidEndPoint()
    {
        const string validHost = "127.0.0.1";
        const int port = 1716;
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