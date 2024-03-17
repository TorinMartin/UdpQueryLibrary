using System.Text;
using FastMember;
using QueryLibrary.Extensions;
using QueryLibrary.Models;

namespace QueryLibrary;

internal static class ResponseParser
{
    public static (TStatusResult Status, List<TPlayerResult>? Players) Parse<TStatusResult, TPlayerResult>(byte[] input, QueryType queryType) 
        where TStatusResult : IServerStatus, new()
        where TPlayerResult : new()
    {
        using var reader = new BinaryReader(new MemoryStream(input), Encoding.UTF8);
            
        var status = ParseStatus<TStatusResult>(reader);

        // Server status is always required as PlayerCount is needed to fetch players
        return queryType switch
        {
            QueryType.Both => (status, ParsePlayers<TPlayerResult>(reader, status.PlayerCount)),
            QueryType.Status => (status, null),
            _ => throw new ArgumentException($"Unsupported query type provided")
        };
    }

    private static TResult ParseStatus<TResult>(BinaryReader reader) where TResult : IServerStatus, new()
    {
        var status = new TResult();
        var accessor = ObjectAccessor.Create(status);

        while (reader.TryReadEx(out var key))
        {
            try
            {
                accessor[key] = reader.ReadEx(key).Trim();
            }
            catch (ArgumentOutOfRangeException)
            {
                // Empty block to allow TResult to not include all fields returned by server
            }
        }

        return status;
    }

    private static List<TResult> ParsePlayers<TResult>(BinaryReader reader, int numPlayers) where TResult : new()
    {
        var players = new List<TResult>();

        // Skip first byte
        reader.ReadByte();

        var playerCount = reader.ReadByte();

        // Count byte may not be returned, if so reverse one byte
        if (playerCount > 64)
        {
            playerCount = (byte)numPlayers;
            reader.BaseStream.Seek(-1, SeekOrigin.Current);
        }

        var keys = new List<string>();
        while (reader.TryReadEx(out var key))
        {
            keys.Add(key.TrimEnd('_'));
        }

        for (var i = 0; i < playerCount; i++)
        {
            var p = new TResult();
            var accessor = ObjectAccessor.Create(p);

            foreach (var key in keys)
            {
                try
                {
                    accessor[key] = reader.ReadEx().Trim();
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Empty block to allow TResult to not include all fields returned by server
                }
            }
                
            players.Add(p);
        }

        return players;
    }
}