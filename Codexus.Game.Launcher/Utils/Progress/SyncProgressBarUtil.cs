using System.Text;

namespace Codexus.Game.Launcher.Utils.Progress;

public static class SyncProgressBarUtil {
    private static readonly Lock Lock = new();

    private static string GetAnsiColorCode(ConsoleColor color)
    {
        return color switch {
            ConsoleColor.Black => "\e[30m",
            ConsoleColor.DarkRed => "\e[31m",
            ConsoleColor.DarkGreen => "\e[32m",
            ConsoleColor.DarkYellow => "\e[33m",
            ConsoleColor.DarkBlue => "\e[34m",
            ConsoleColor.DarkMagenta => "\e[35m",
            ConsoleColor.DarkCyan => "\e[36m",
            ConsoleColor.Gray => "\e[37m",
            ConsoleColor.DarkGray => "\e[90m",
            ConsoleColor.Red => "\e[91m",
            ConsoleColor.Green => "\e[92m",
            ConsoleColor.Yellow => "\e[93m",
            ConsoleColor.Blue => "\e[94m",
            ConsoleColor.Magenta => "\e[95m",
            ConsoleColor.Cyan => "\e[96m",
            ConsoleColor.White => "\e[97m",
            _ => "\e[37m"
        };
    }

    public class ProgressBarOptions {
        public int Width { get; set; } = 45;

        public char FillChar { get; set; } = '■';

        public char EmptyChar { get; set; } = '·';

        public string ProgressFormat { get; set; } = "{0:P1}";

        public bool ShowPercentage { get; set; } = true;

        public bool ShowElapsedTime { get; set; } = true;

        public bool ShowEta { get; set; } = true;

        public bool ShowSpinner { get; set; } = true;

        public ConsoleColor FillColor { get; set; } = ConsoleColor.Cyan;

        public ConsoleColor EmptyColor { get; set; } = ConsoleColor.DarkGray;

        public ConsoleColor SpinnerColor { get; set; } = ConsoleColor.Cyan;

        public string Prefix { get; set; } = "";

        public string Suffix { get; set; } = "";

        public bool LastLineNewline { get; set; } = true;
    }

    public class ProgressBar(int total = 100, ProgressBarOptions? options = null) : IDisposable {
        private readonly ProgressBarOptions _options = options ?? new ProgressBarOptions();

        private readonly char[] _spinnerChars = ['|', '/', '─', '\\'];

        private readonly DateTime _startTime = DateTime.Now;

        private double _current;

        private bool _disposed;

        private int _spinnerIndex;

        public void Dispose()
        {
            if (!_disposed) {
                if (_current < total) Update(total, "Done");
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // Heypixel Protocol 特供
        public void Update(int current, string action)
        {
            Update((double)current, action);
        }

        public void Update(double current, string action)
        {
            if (_disposed) return;
            _current = current;
            _spinnerIndex = (_spinnerIndex + 1) % _spinnerChars.Length;
            Display(action);
        }

        private void Display(string action)
        {
            using (Lock.EnterScope()) {
                ClearCurrent();
                var num = _current / total;
                var num2 = (int)(num * _options.Width);
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(_options.Prefix);
                if (_options.ShowSpinner) {
                    if (_current < total) {
                        stringBuilder.Append(GetAnsiColorCode(_options.SpinnerColor));
                        stringBuilder.Append(_spinnerChars[_spinnerIndex]);
                        stringBuilder.Append(GetAnsiColorCode(ConsoleColor.White));
                        stringBuilder.Append(' ');
                    } else if (_current >= total) {
                        stringBuilder.Append(GetAnsiColorCode(ConsoleColor.Green));
                        stringBuilder.Append('✓');
                        stringBuilder.Append(GetAnsiColorCode(ConsoleColor.White));
                        stringBuilder.Append(' ');
                    }
                }

                stringBuilder.Append('[');
                stringBuilder.Append(GetAnsiColorCode(_options.FillColor));
                stringBuilder.Append(new string(_options.FillChar, num2));
                stringBuilder.Append(GetAnsiColorCode(_options.EmptyColor));
                stringBuilder.Append(new string(_options.EmptyChar, _options.Width - num2));
                stringBuilder.Append(GetAnsiColorCode(ConsoleColor.White));
                var stringBuilder2 = stringBuilder;
                var stringBuilder3 = stringBuilder2;
                var handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
                handler.AppendLiteral("] ");
                handler.AppendFormatted(action);
                stringBuilder3.Append(ref handler);
                if (_options.ShowPercentage) {
                    stringBuilder2 = stringBuilder;
                    var stringBuilder4 = stringBuilder2;
                    handler = new StringBuilder.AppendInterpolatedStringHandler(1, 1, stringBuilder2);
                    handler.AppendLiteral(" ");
                    handler.AppendFormatted(string.Format(_options.ProgressFormat, num));
                    stringBuilder4.Append(ref handler);
                }

                var value = DateTime.Now - _startTime;
                if (_options.ShowElapsedTime) {
                    stringBuilder2 = stringBuilder;
                    var stringBuilder5 = stringBuilder2;
                    handler = new StringBuilder.AppendInterpolatedStringHandler(10, 1, stringBuilder2);
                    handler.AppendLiteral(" Elapsed: ");
                    handler.AppendFormatted(value, "mm\\:ss");
                    stringBuilder5.Append(ref handler);
                }

                if (_options.ShowEta && _current > 0) {
                    var value2 = TimeSpan.FromMilliseconds(value.TotalMilliseconds / _current * (total - _current));
                    stringBuilder2 = stringBuilder;
                    var stringBuilder6 = stringBuilder2;
                    handler = new StringBuilder.AppendInterpolatedStringHandler(6, 1, stringBuilder2);
                    handler.AppendLiteral(" ETA: ");
                    handler.AppendFormatted(value2, "mm\\:ss");
                    stringBuilder6.Append(ref handler);
                }

                stringBuilder.Append(_options.Suffix);
                stringBuilder.Append("\e[0m");
                Console.Write($"\r{stringBuilder}");
                if (_current >= total && _options.LastLineNewline) Console.WriteLine();
            }
        }

        public static void ClearCurrent()
        {
            Console.Write("\r" + new string(' ', Console.BufferWidth) + "\r");
        }
    }

    public class ProgressReport {
        public double Percent { get; set; }

        public string Message { get; set; } = "";
    }
}