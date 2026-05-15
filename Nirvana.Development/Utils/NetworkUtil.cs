using System.Linq;
using System.Net.NetworkInformation;

namespace Nirvana.Development.Utils;

public static class NetworkUtil {
    public static int GetAvailablePort(int low = 25565)
    {
        for (var i = low; i < 65535; i++) {
            if (IsPortAvailable(i)) {
                return i;
            }
        }

        return -1;
    }

    private static bool IsPortAvailable(int port)
    {
        var properties = IPGlobalProperties.GetIPGlobalProperties();
        var activeListeners = properties.GetActiveTcpListeners();
        return activeListeners.All(endpoint => endpoint.Port != port);
    }
}