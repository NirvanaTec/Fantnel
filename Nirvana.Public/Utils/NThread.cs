namespace Nirvana.Public.Utils;

public class NThread {
    public static void Start(Action action, Action<Exception>? onError = null, Action? onSuccess = null)
    {
        _= Task.Run(() => {
            var thread1 = new Thread(() => {
                try {
                    action.Invoke();
                } catch (Exception e) {
                    onError?.Invoke(e);
                }
            });
            thread1.Start();
            thread1.Join();
            onSuccess?.Invoke();
        });
    }
}