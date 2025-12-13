using Codexus.Cipher.Protocol;
using Codexus.OpenSDK;
using Codexus.OpenSDK.Yggdrasil;

namespace NirvanaPublic.Utils;

public record Services(
    C4399 C4399,
    X19 X19,
    WPFLauncher Wpf,
    StandardYggdrasil Yggdrasil
);