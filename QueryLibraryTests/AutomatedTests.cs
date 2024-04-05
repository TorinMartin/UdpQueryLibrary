using NUnit.Framework;
using QueryLibrary.Models;
using QueryLibrary.Services;
using QueryLibraryTests.Model;

namespace QueryLibraryTests;

public class AutomatedTests
{
    private const string ValidServerHost = "45.77.52.236";
    private const int ValidServerPort = 1776;
    
    private IQueryRunner _queryRunner = null!;
    private QueryLibrary.IQueryLibrary _queryLibrary = null!;
    
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var helper = new QueryHelper();
        _queryRunner = new QueryRunner(helper);

        _queryLibrary = new QueryLibrary.QueryLibrary(_queryRunner, helper);

    }
    
    [Test]
    public async Task AutomatedTest_Returns_Status_Players()
    {
        var result = await _queryLibrary.RunAsync<ServerStatus, Player>(ValidServerHost, ValidServerPort);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.Not.Null);
            Assert.That(result.Players, Is.Not.Null);
        });
    }

    [Test]
    public async Task AutomatedTest_Players_Is_Null()
    {
        var result = await _queryLibrary.RunAsync<ServerStatus, Player>(ValidServerHost, ValidServerPort, QueryType.Status);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.Not.Null);
            Assert.That(result.Players, Is.Null);
        });
    }
}