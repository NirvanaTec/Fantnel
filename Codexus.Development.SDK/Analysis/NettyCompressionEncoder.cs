using Codexus.Development.SDK.Extensions;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;

namespace Codexus.Development.SDK.Analysis;

public class NettyCompressionEncoder(int threshold) : MessageToByteEncoder<IByteBuffer> {
    public int Threshold { get; set; } = threshold;

    protected override void Encode(IChannelHandlerContext context, IByteBuffer message, IByteBuffer output)
    {
        var readableBytes = message.ReadableBytes;

        // 如果消息长度小于阈值，则不进行压缩，标记长度为0
        if (readableBytes < Threshold) {
            output.WriteVarInt(0);
            output.WriteBytes(message); // 直接将原消息写入输出
            return;
        }

        // 写入压缩后的数据长度占位符（稍后会被覆盖）
        output.WriteVarInt(readableBytes);

        // 将输入的 IByteBuffer 内容复制到内存流中，准备进行压缩
        using var inputStream = new MemoryStream();
        // 将 message 的可读部分写入内存流
        // 注意：这里使用 WriteBytes(message) 是一种更高效的方式
        // 它会自动处理 message 的 ReaderIndex 和 WritableBytes
        inputStream.Write(message.Array, message.ArrayOffset + message.ReaderIndex, message.ReadableBytes);
        // 重要：必须重置流的位置到开头，否则压缩流会从末尾开始读取
        inputStream.Position = 0;

        // 创建一个内存流用于接收压缩后的数据
        using var outputStream = new MemoryStream();
        // 使用 SharpCompress 的 ZlibStream 进行压缩
        // CompressionMode.Compress 表示我们要压缩数据
        // leaveOpen: true 表示不关闭底层的 outputStream
        using (var compressorStream = new ZlibStream(outputStream, CompressionMode.Compress)) {
            // 将输入流的内容通过压缩流写入输出流
            inputStream.CopyTo(compressorStream);
            // 确保所有数据都被压缩并写入到底层输出流
            compressorStream.Flush();
        }

        // 获取压缩后的字节数组
        var compressedData = outputStream.ToArray();

        // 将压缩后的数据写入最终的 Netty 输出缓冲区
        output.WriteBytes(compressedData);

        // 更新 message 的 ReaderIndex，表示这部分数据已经被消费
        // 原始代码在这里执行了此操作，但通常 MessageToByteEncoder 会自动处理
        // 如果原始逻辑依赖于此，请取消注释下一行
        // message.SetReaderIndex(message.ReaderIndex + message.ReadableBytes);
    }
}