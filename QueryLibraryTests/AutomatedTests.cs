using NUnit.Framework;
using QueryLibrary;
using QueryLibrary.Models;
using QueryLibraryTests.Model;

namespace QueryLibraryTests;

public class AutomatedTests
{
    private const string ValidServerHost = "66.135.10.109";
    private const int ValidServerPort = 1726;
    
    private QueryRunner _queryRunner = null!;
    
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _queryRunner = new QueryRunner(portOffset: 1);
    }
    
    [Test]
    public async Task AutomatedTest_Returns_Status_Players()
    {
        var result = await _queryRunner.RunAsync<ServerStatus, Player>(ValidServerHost, ValidServerPort);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.Not.Null);
            Assert.That(result.Players, Is.Not.Null);
        });
    }

    [Test]
    public async Task AutomatedTest_Players_Is_Null()
    {
        var result = await _queryRunner.RunAsync<ServerStatus, Player>(ValidServerHost, ValidServerPort, QueryType.Status);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.Not.Null);
            Assert.That(result.Players, Is.Null);
        });
    }
}