using System.Text;

namespace Codexus.Game.Launcher.Services.Java.RPC.Events;

public static class SimplePack {
    public static byte[]? Pack(params object[]? data)
    {
        if (data == null) {
            return null;
        }

        var array = Array.Empty<byte>();
        foreach (var obj in data) {
            var array2 = Array.Empty<byte>();
            var type = obj.GetType();
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Object:
                    if (type == typeof(byte[])) {
                        array2 = (byte[])obj;
                    } else if (type == typeof(List<uint>)) {
                        if (obj is not List<uint> uints) {
                            continue;
                        }

                        var bytes = BitConverter.GetBytes((ushort)(uints.Count * 4));
                        var list = new List<byte>();
                        list.AddRange(array2);
                        list.AddRange(bytes);
                        foreach (var item in uints) {
                            list.AddRange(BitConverter.GetBytes(item));
                        }

                        array2 = list.ToArray();
                    } else if (type == typeof(List<ulong>)) {
                        if (obj is not List<ulong> ulongs) {
                            continue;
                        }

                        var bytes2 = BitConverter.GetBytes((ushort)(ulongs.Count * 8));
                        var list2 = new List<byte>();
                        list2.AddRange(array2);
                        list2.AddRange(bytes2);
                        foreach (var item2 in ulongs) list2.AddRange(BitConverter.GetBytes(item2));
                        array2 = list2.ToArray();
                    } else {
                        if (obj is not List<ulong> longs) {
                            break;
                        }

                        var list3 = new List<byte>();
                        var bytes3 = BitConverter.GetBytes((ushort)(longs.Count * 8));
                        list3.AddRange(array2);
                        list3.AddRange(bytes3);
                        foreach (var item3 in longs) list3.AddRange(BitConverter.GetBytes(item3));
                        array2 = list3.ToArray();
                    }

                    break;
                case TypeCode.Boolean:
                    array2 = BitConverter.GetBytes((bool)obj);
                    break;
                case TypeCode.Byte:
                    array2 = [(byte)obj];
                    break;
                case TypeCode.Int16:
                    array2 = BitConverter.GetBytes((short)obj);
                    break;
                case TypeCode.UInt16:
                    array2 = BitConverter.GetBytes((ushort)obj);
                    break;
                case TypeCode.Int32:
                    array2 = BitConverter.GetBytes((int)obj);
                    break;
                case TypeCode.UInt32:
                    array2 = BitConverter.GetBytes((uint)obj);
                    break;
                case TypeCode.Int64:
                    array2 = BitConverter.GetBytes((long)obj);
                    break;
                case TypeCode.Double:
                    array2 = BitConverter.GetBytes((double)obj);
                    break;
                case TypeCode.String:
                    array2 = Encoding.UTF8.GetBytes((string)obj);
                    array2 = Pack((ushort)array2.Length, array2);
                    break;
            }

            if (array2 != null) array = array.Concat(array2).ToArray();
        }

        return array;
    }
}