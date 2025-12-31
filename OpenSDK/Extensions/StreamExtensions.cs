using System.Net.Sockets;

namespace OpenSDK.Extensions;

public static class StreamExtensions
{
    extension(NetworkStream stream)
    {
        public async Task<MemoryStream> ReadSteamWithInt16Async()
        {
            var lengthBytes = new byte[2];
            var bytesRead = await stream.ReadAsync(lengthBytes);

            if (bytesRead != 2) throw new EndOfStreamException("Could not read the length prefix.");
            var length = BitConverter.ToInt16(lengthBytes, 0);

            if (length < 0) throw new InvalidDataException("Length cannot be negative.");
            var memoryStream = new MemoryStream(length);

            var buffer = new byte[1024];
            int remainingBytes = length;

            while (remainingBytes > 0)
            {
                var bytesToRead = Math.Min(buffer.Length, remainingBytes);
                var read = await stream.ReadAsync(buffer.AsMemory(0, bytesToRead));

                if (read == 0) throw new EndOfStreamException("End of stream reached before reading complete data.");

                memoryStream.Write(buffer, 0, read);
                remainingBytes -= read;
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}