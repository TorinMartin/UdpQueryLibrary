using NUnit.Framework;
using QueryLibrary;
using QueryLibraryTests.Model;

namespace QueryLibraryTests;

public class UnitTests
{
    private QueryRunner _queryRunner = null!;
    
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _queryRunner = new QueryRunner(portOffset: 1);
    }

    [Test]
    public void Invalid_Host_Throws()
    {
        Assert.That(() => _queryRunner.RunAsync<ServerStatus, Player>("invalid", 1716), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Request_Times_Out()
    {
        _queryRunner.TimeoutMs = 1;
        Assert.That(() => _queryRunner.RunAsync<ServerStatus, Player>("127.0.0.1", 1716), Throws.TypeOf<TimeoutException>());
    }
}