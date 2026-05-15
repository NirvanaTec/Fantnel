using DotNetty.Transport.Channels;
using Nirvana.DevPlugin.Entities;
using Nirvana.DevPlugin.Enums;

namespace Nirvana.DevPlugin;

public abstract class BGameConnection {
    public IChannel? ClientChannel;
    public required InterceptorConfig Config;
    public EnumProtocolVersion ProtocolVersion;
    public IChannel? ServerChannel;
    public EnumConnectionState State;
}