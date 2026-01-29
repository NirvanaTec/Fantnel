namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.RentalGame;

public enum EnumServerStatus
{
    None = -1, // 0xFFFFFFFF
    ServerOff = 0,
    ServerOn = 1,
    Uninitialized = 2,
    Opening = 3,
    Closing = 4,
    OutOfDate = 5,
    SaveCleaning = 6,
    Resetting = 7,
    Upgrading = 8,
    DiscOverflow = 9,
}
