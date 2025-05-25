using System.IO.Compression;

namespace GraphSearch.ConsoleApp.Helpers;

public static class CompressionHelper
{
    public static byte[] Compress(byte[] uncompressedBytes)
    {
        using MemoryStream compressedStream = new();
        using GZipStream gzipStream = new(compressedStream, CompressionLevel.Optimal);
        gzipStream.Write(uncompressedBytes);
        gzipStream.Flush();
        return compressedStream.ToArray();
    }

    public static byte[] Decompress(byte[] compressedBytes)
    {
        using MemoryStream compressedStream = new(compressedBytes);
        using GZipStream gzipStream = new(compressedStream, CompressionMode.Decompress);
        using MemoryStream decompressedStream = new();
        gzipStream.CopyTo(decompressedStream);
        return decompressedStream.ToArray();
    }
}