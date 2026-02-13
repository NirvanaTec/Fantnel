using System.Reflection;
using Codexus.Development.SDK.Attributes;
using Codexus.Development.SDK.Enums;

namespace Codexus.Development.SDK.RakNet;

public static class RakNetLoader {
    private static Type? _loader;

    public static void FindLoader()
    {
        var types = Assembly.LoadFrom("Codexus.RakNet.ug").GetTypes();
        foreach (var type in types) {
            var customAttribute = type.GetCustomAttribute<ComponentLoader>(false);
            if (customAttribute is { Type: EnumLoaderType.RakNet } &&
                typeof(IRakNetCreate).IsAssignableFrom(type)) {
                _loader = type;
                break;
            }
        }

        if (_loader == null) {
            throw new Exception("Could not initialize RakNet");
        }
    }

    public static IRakNetCreate? ConstructLoader()
    {
        if (_loader == null) {
            throw new Exception("You must call FindLoader() before ConstructLoader()");
        }

        return (IRakNetCreate?)Activator.CreateInstance(_loader);
    }
}