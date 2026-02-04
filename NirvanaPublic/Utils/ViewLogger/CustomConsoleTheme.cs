using Serilog.Sinks.SystemConsole.Themes;

namespace NirvanaPublic.Utils.ViewLogger;

public class CustomConsoleTheme : ConsoleTheme {
    private readonly ConsoleColor _color;

    public CustomConsoleTheme(ConsoleColor color)
    {
        ResetCharCount = 0;
        _color = color;
    }

    protected override int ResetCharCount { get; } = -1;

    public override bool CanBuffer => false;

    public override int Set(TextWriter output, ConsoleThemeStyle style)
    {
        switch (style) {
            case ConsoleThemeStyle.TertiaryText or ConsoleThemeStyle.SecondaryText:
                Console.ForegroundColor = _color;
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