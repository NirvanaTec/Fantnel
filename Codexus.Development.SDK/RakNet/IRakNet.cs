namespace Codexus.Development.SDK.RakNet;

public interface IRakNet {
    Guid GetId();

    string GetRemoteAddress();

    string GetLocalAddress();

    string GetRoleName();

    string GetGameName();

    void Shutdown();
}