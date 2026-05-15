using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nirvana.Game.Launcher.Services.Java.RPC.Events;

public static class SimplePack {
    public static byte[]? Pack(params object[]? data)
    {
        if (data == null) {
            return null;
        }

        var array = Array.Empty<byte>();
        foreach (var obj in data) {
            var array2 = Array.Empty<byte>();
            switch (obj) {
                case byte[] bytes1:
                    array2 = bytes1;
                    break;
                case List<uint> uints: {
                    var list = new List<byte>();
                    var bytes = BitConverter.GetBytes((ushort)(uints.Count * 4));
                    list.AddRange(array2);
                    list.AddRange(bytes);
                    foreach (var item in uints) {
                        list.AddRange(BitConverter.GetBytes(item));
                    }
                    array2 = list.ToArray();
                    break;
                }
                case List<ulong> ulongs: {
                    var list2 = new List<byte>();
                    var bytes2 = BitConverter.GetBytes((ushort)(ulongs.Count * 8));
                    list2.AddRange(array2);
                    list2.AddRange(bytes2);
                    foreach (var item2 in ulongs) {
                        list2.AddRange(BitConverter.GetBytes(item2));
                    }
                    array2 = list2.ToArray();
                    break;
                }
                case List<long> longs: {
                    var list3 = new List<byte>();
                    var bytes3 = BitConverter.GetBytes((ushort)(longs.Count * 8));
                    list3.AddRange(array2);
                    list3.AddRange(bytes3);
                    foreach (var item3 in longs) {
                        list3.AddRange(BitConverter.GetBytes(item3));
                    }
                    array2 = list3.ToArray();
                    break;
                }
                case bool boolValue:
                    array2 = BitConverter.GetBytes(boolValue);
                    break;
                case byte byteValue:
                    array2 = [byteValue];
                    break;
                case short shortValue:
                    array2 = BitConverter.GetBytes(shortValue);
                    break;
                case ushort ushortValue:
                    array2 = BitConverter.GetBytes(ushortValue);
                    break;
                case int intValue:
                    array2 = BitConverter.GetBytes(intValue);
                    break;
                case uint uintValue:
                    array2 = BitConverter.GetBytes(uintValue);
                    break;
                case long longValue:
                    array2 = BitConverter.GetBytes(longValue);
                    break;
                case ulong ulongValue:
                    array2 = BitConverter.GetBytes(ulongValue);
                    break;
                case float floatValue:
                    array2 = BitConverter.GetBytes(floatValue);
                    break;
                case double doubleValue:
                    array2 = BitConverter.GetBytes(doubleValue);
                    break;
                case string stringValue:
                    array2 = Encoding.UTF8.GetBytes(stringValue);
                    array2 = Pack((ushort)array2.Length, array2);
                    break;
            }
            if (array2 != null) {
                array = array.Concat(array2).ToArray();
            }
        }
        return array;
    }
}