using Codexus.Development.SDK.Entities;

namespace Codexus.Development.SDK.Manager;

public interface IUserManager {
    static IUserManager? Instance;

    static IUserManager? CppInstance;

    EntityAvailableUser GetAvailableUser(string entityId);
}