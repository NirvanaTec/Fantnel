namespace WPFLauncherApi.Entities;

public class EntityX19Exception(string message, object entityWpfLauncher) : Exception(message) {
    public new object Data { get; } = entityWpfLauncher;
}