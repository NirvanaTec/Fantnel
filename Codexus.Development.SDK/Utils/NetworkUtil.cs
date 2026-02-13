using System.Net.NetworkInformation;

namespace Codexus.Development.SDK.Utils;

public static class NetworkUtil {
    public static int GetAvailablePort(int low = 25565, int high = 35565, bool reuseTimeWait = false)
    {
        if (low > high) {
            return 0;
        }

        var usedPorts = GetUsedPorts(reuseTimeWait);
        for (var i = low; i <= high; i++) {
            if (!usedPorts.Contains(i)) {
                return i;
            }
        }

        return 0;
    }

    private static HashSet<int> GetUsedPorts(bool reuseTimeWait = true)
    {
        var iPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        var first = from e in iPGlobalProperties.GetActiveTcpListeners()
            select e.Port;
        var second = from e in iPGlobalProperties.GetActiveUdpListeners()
            select e.Port;
        var activeTcpConnections = iPGlobalProperties.GetActiveTcpConnections();
        IEnumerable<TcpConnectionInformation> source;
        if (!reuseTimeWait) {
            IEnumerable<TcpConnectionInformation> enumerable = activeTcpConnections;
            source = enumerable;
        } else {
            source = activeTcpConnections.Where(c => c.State != TcpState.TimeWait && c.State != TcpState.CloseWait);
        }

        var second2 = source.Select(c => c.LocalEndPoint.Port);
        var hashSet = new HashSet<int>();
        foreach (var item in first.Concat(second).Concat(second2)) {
            hashSet.Add(item);
        }

        return hashSet;
    }
}