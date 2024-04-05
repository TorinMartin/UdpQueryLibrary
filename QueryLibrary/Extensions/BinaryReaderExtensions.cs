using System.Text;

namespace QueryLibrary.Extensions;

public static class BinaryReaderExtensions
{
    public static string ReadEx(this BinaryReader br, string? key = null)
    {
        var bytes = new List<byte>();
        
        byte current;
        while (br.BaseStream.Position < br.BaseStream.Length && (current = br.ReadByte()) != 0)
        {
            bytes.Add(current);
        }

        return Encoding.UTF8.GetString(bytes.ToArray());
    }

    public static bool TryReadEx(this BinaryReader br, out string result)
    {
        result = br.ReadEx();
        return string.IsNullOrEmpty(result) is false;
    }
}