using QueryLibrary.Models;

namespace QueryLibraryTests.Model;

public class ServerStatus : IServerStatus
{
    public int PlayerCount => int.Parse(numplayers);

    public string hostname { get; set; } = string.Empty;
    public string numplayers { get; set; } = string.Empty;
    public string mapname { get; set; } = string.Empty;
    public string hostport { get; set; }
}