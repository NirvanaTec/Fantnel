using System.Reflection;
using NirvanaAPI.Utils;

namespace Nirvana.Public.Entities.Plugin;

public class EntityPluginAssembly(string pluginPath, Assembly assembly) {
    private readonly string _sha256 = Tools.ComputeSha256(pluginPath);

    public readonly Assembly Assembly = assembly;

    public bool Equals(string pluginPath)
    {
        return _sha256.Equals(Tools.ComputeSha256(pluginPath));
    }
}