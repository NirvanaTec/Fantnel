namespace Codexus.Development.SDK.RakNet;

public interface IRakNetCreate {
    IRakNet Create(string remoteAddress, string nexusToken, string serverId, uint userId, string userToken,
        string serverName, string roleName, int port, int ipv6Port, bool isRental);
}