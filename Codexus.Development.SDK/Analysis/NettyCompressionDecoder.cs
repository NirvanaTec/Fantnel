using System.Buffers;
using Codexus.Development.SDK.Extensions;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;

namespace Codexus.Development.SDK.Analysis;

/// <summary>
///     使用 SharpCompress 库进行解压缩的 Netty 解码器。
///     注意：SharpCompress 的 ZlibStream 用于处理原始 Zlib 数据流。
/// </summary>
public class NettyCompressionDecoder(int threshold) : ByteToMessageDecoder {
    private const int InitialBufferSize = 4096;

    // 使用共享的字节数组池来管理临时缓冲区
    private readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;

    public int Threshold { get; set; } = threshold;

    protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
    {
        if (input.ReadableBytes == 0) {
            return;
        }

        // 读取解压缩后的预期长度
        var decompressedLength = input.ReadVarIntFromBuffer();

        // 如果长度为0，则表示数据未被压缩，直接传递
        if (decompressedLength == 0) {
            output.Add(input.ReadBytes(input.ReadableBytes));
            return;
        }

        // 检查解压缩后长度是否低于阈值
        if (decompressedLength < Threshold) {
            throw new DecoderException($"Decompressed length {decompressedLength} is below threshold {Threshold}");
        }

        // 将可读的压缩数据读入临时数组
        var compressedData = new byte[input.ReadableBytes];
        input.ReadBytes(compressedData);

        // 从池中租借一个足够大的缓冲区用于解压输出
        var decompressedBuffer = _arrayPool.Rent(Math.Max(InitialBufferSize, decompressedLength));

        try {
            using var inputStream = new MemoryStream(compressedData);
            // 使用 SharpCompress 的 ZlibStream 进行解压
            // CompressionMode.Decompress 表示我们要解压数据
            // leaveOpen: true 表示不关闭底层的 inputStream
            using var decompressorStream = new ZlibStream(inputStream, CompressionMode.Decompress);

            var totalBytesRead = 0;
            // 循环读取，直到填满预期大小的缓冲区或流结束
            while (totalBytesRead < decompressedLength) {
                // 计算下一次最多能读多少字节，防止越界
                var bytesToRead = Math.Min(decompressedBuffer.Length - totalBytesRead,
                    decompressedLength - totalBytesRead);

                var lastBytesRead = decompressorStream.Read(decompressedBuffer, totalBytesRead, bytesToRead);

                // 如果读不到任何字节，但还没达到预期长度，说明数据不完整或有误
                if (lastBytesRead == 0) {
                    throw new DecoderException("Incomplete compressed data or stream ended prematurely.");
                }

                totalBytesRead += lastBytesRead;
            }

            // 验证解压后的实际长度与预期长度是否一致
            if (totalBytesRead != decompressedLength) {
                // 理论上，在上述循环后这个条件不会成立，但作为双重保险
                throw new DecoderException(
                    $"Decompressed length mismatch: expected {decompressedLength}, got {totalBytesRead}");
            }

            // 使用 Netty 分配器创建一个新的 HeapBuffer，
            // 并将解压后的数据写入其中
            var resultBuffer = context.Allocator.HeapBuffer(decompressedLength);
            resultBuffer.WriteBytes(decompressedBuffer, 0, totalBytesRead);

            // 将结果缓冲区添加到输出列表
            output.Add(resultBuffer);
        } catch (InvalidDataException ex) {
            // ZlibStream 在遇到无效/不完整数据时可能会抛出此异常
            throw new DecoderException("Failed to decompress data", ex);
        } finally {
            // 最终将租借的缓冲区归还给池，防止内存泄漏
            _arrayPool.Return(decompressedBuffer);
        }
    }
}