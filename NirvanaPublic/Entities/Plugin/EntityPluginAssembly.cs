using System.Reflection;
using NirvanaAPI.Utils;

namespace NirvanaPublic.Entities.Plugin;

public class EntityPluginAssembly(string pluginPath, Assembly assembly) {

    public readonly Assembly Assembly = assembly;
    private readonly string _sha256 = Tools.ComputeSha256(pluginPath);

    public bool Equals(string pluginPath)
    {
        return _sha256.Equals(Tools.ComputeSha256(pluginPath));
    }
    
}