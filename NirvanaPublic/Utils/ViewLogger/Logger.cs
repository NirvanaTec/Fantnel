using Serilog;
using Serilog.Events;

namespace NirvanaPublic.Utils.ViewLogger;

public class Logger : LoggerConfiguration
{
    public void SetColor(LogEventLevel level, ConsoleColor color)
    {
        WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(evt => evt.Level == level).WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss}] {Message:lj}{NewLine}{Exception}",
            theme: new CustomConsoleTheme(color)
        ));
    }
}