using Serilog.Sinks.SystemConsole.Themes;

namespace NirvanaPublic.Utils.ViewLogger;

public class CustomConsoleTheme(ConsoleColor color) : ConsoleTheme {
    
    protected override int ResetCharCount => 0;

    public override bool CanBuffer => false;

    public override int Set(TextWriter output, ConsoleThemeStyle style)
    {
        switch (style) {
            case ConsoleThemeStyle.TertiaryText or ConsoleThemeStyle.SecondaryText:
                Console.ForegroundColor = color;
                break;
            default:
                Console.ResetColor();
                break;
        }

        return 0;
    }

    public override void Reset(TextWriter output)
    {
        Console.ResetColor();
    }
    
}