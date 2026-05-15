using System;
using System.Buffers;
using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using ICSharpCode.SharpZipLib.Zip.Compression;
using Nirvana.DevPlugin.Extensions;

namespace Nirvana.Development.Analysis;

public class NettyCompressionDecoder(int threshold) : ByteToMessageDecoder {
    private readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;

    private readonly Inflater _inflater = new();

    public int Threshold { get; set; } = threshold;

    protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
    {
        if (input.ReadableBytes == 0) {
            return;
        }

        var num = input.ReadVarIntFromBuffer();
        if (num == 0) {
            output.Add(input.ReadBytes(input.ReadableBytes));
            return;
        }

        if (num < Threshold) {
            throw new DecoderException($"Decompressed length {num} is below threshold {Threshold}");
        }

        var array = new byte[input.ReadableBytes];
        input.ReadBytes(array);
        var array2 = _arrayPool.Rent(Math.Max(4096, num));
        try {
            _inflater.Reset();
            _inflater.SetInput(array);
            if (_inflater.IsNeedingDictionary) {
                throw new DecoderException("Inflater requires dictionary");
            }

            var byteBuffer = context.Allocator.HeapBuffer(num);
            var num2 = 0;
            while (!_inflater.IsFinished && num2 < num) {
                var num3 = _inflater.Inflate(array2);
                if (num3 == 0 && _inflater.IsNeedingInput) {
                    throw new DecoderException("Incomplete compressed data");
                }

                byteBuffer.WriteBytes(array2, 0, num3);
                num2 += num3;
            }

            if (num2 != num) {
                byteBuffer.Release();
                throw new DecoderException($"Decompressed length mismatch: expected {num}, got {num2}");
            }

            output.Add(byteBuffer);
        } finally {
            _arrayPool.Return(array2);
        }
    }
}