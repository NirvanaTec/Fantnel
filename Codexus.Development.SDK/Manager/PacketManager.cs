using System.Reflection;
using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;
using Serilog;

namespace Codexus.Development.SDK.Manager;

public class PacketManager {
    private static PacketManager? _instance;

    private readonly Dictionary<Type, Dictionary<EnumProtocolVersion, int>> _ids = new();

    private readonly Dictionary<Type, RegisterPacket> _metadata = new();

    private readonly
        Dictionary<EnumConnectionState,
            Dictionary<EnumPacketDirection, Dictionary<EnumProtocolVersion, Dictionary<int, List<Type>>>>> _packets =
            new();

    private readonly bool _registered;

    private readonly Dictionary<Type, EnumConnectionState> _states = new();

    private PacketManager()
    {
        RegisterDefaultPackets();
        _registered = true;
    }

    public static PacketManager Instance => _instance ??= new PacketManager();

    private void RegisterDefaultPackets()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies) {
            RegisterPacketFromAssembly(assembly);
        }
    }

    public void RegisterPacketFromAssembly(Assembly assembly)
    {
        foreach (var item in from type in assembly.GetTypes()
                 where typeof(IPacket).IsAssignableFrom(type) && type is { IsAbstract: false, IsInterface: false }
                 select type) {
            var list = item.GetCustomAttributes<RegisterPacket>(false).ToList();
            if (list.Count == 0) {
                continue;
            }

            foreach (var item2 in list) {
                RegisterPacket(item2, item);
            }
        }
    }

    private void RegisterPacket(RegisterPacket metadata, Type type)
    {
        if (type.GetConstructor(Type.EmptyTypes) == null) {
            throw new InvalidOperationException("Type '" + type.FullName +
                                                "' does not have a parameterless constructor.");
        }

        _states[type] = metadata.State;
        _metadata[type] = metadata;
        if (!_ids.TryGetValue(type, out var value)) {
            value = new Dictionary<EnumProtocolVersion, int>();
            _ids[type] = value;
        }

        if (!_packets.TryGetValue(metadata.State, out var value2)) {
            value2 =
                new Dictionary<EnumPacketDirection, Dictionary<EnumProtocolVersion, Dictionary<int, List<Type>>>>();
            _packets[metadata.State] = value2;
        }

        if (!value2.TryGetValue(metadata.Direction, out var value3)) {
            value3 = new Dictionary<EnumProtocolVersion, Dictionary<int, List<Type>>>();
            value2[metadata.Direction] = value3;
        }

        var versions = metadata.Versions;
        for (var i = 0; i < versions.Length; i++) {
            var key = versions[i];
            var packetIds = metadata.PacketIds;
            var num = Math.Min(packetIds.Length - 1, i);
            var num2 = packetIds[num]; // This is the packetId

            if (!value3.TryGetValue(key, out var value4)) {
                value4 = value3[key] = new Dictionary<int, List<Type>>(); // 初始化内层字典
            }

            // 处理 packetId -> List<Type> 的映射
            if (!value4.TryGetValue(num2, out var typeList)) {
                typeList = [];
                value4[num2] = typeList; // 如果该 packetId 尚未存在，则创建新列表
            }

            // 将当前 Type 添加到列表中
            if (!typeList.Contains(type)) {
                // 可选：避免重复添加同一个 Type 到同一个 ID 下
                typeList.Add(type);
            }

            // _ids 字典中只记录第一个注册的 ID（或按需更新逻辑）
            // 注意：如果一个 Type 注册了多次，这里可能需要更复杂的处理
            if (!value.TryGetValue(key, out var value1) || value1 != num2) {
                value1 = num2;
                value[key] = value1;
            }
        }
    }

    public List<IPacket?>? BuildPacket(EnumConnectionState state, EnumPacketDirection direction,
        EnumProtocolVersion version,
        int packetId)
    {
        if (!_packets.TryGetValue(state, out var value) || !value.TryGetValue(direction, out var value2) ||
            !value2.TryGetValue(version, out var value3) || !value3.TryGetValue(packetId, out var value4)) {
            return null;
        }

        try {
            return value4.Select(type => (IPacket?)Activator.CreateInstance(type)).ToList();
        } catch (Exception exception) {
            Log.Error(exception, "Failed to build packet");
            return null;
        }
    }

    public int GetPacketId(EnumProtocolVersion version, IPacket packet)
    {
        var type = packet.GetType();
        if (!_ids.TryGetValue(type, out var value)) {
            return -1;
        }

        return value.GetValueOrDefault(version, -1);
    }

    public RegisterPacket? GetMetadata(IPacket packet)
    {
        var type = packet.GetType();
        return _metadata.GetValueOrDefault(type);
    }

    public EnumConnectionState GetState(IPacket packet)
    {
        var type = packet.GetType();
        return _states.GetValueOrDefault(type);
    }

    public void EnsureRegistered()
    {
        if (_registered) {
            return;
        }

        throw new InvalidOperationException("Should never call CheckIsRegistered()");
    }
}