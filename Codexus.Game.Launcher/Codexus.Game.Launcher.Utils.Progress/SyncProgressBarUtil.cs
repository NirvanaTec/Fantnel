using System;
using System.Text;
using System.Threading;

namespace Codexus.Game.Launcher.Utils.Progress;

public static class SyncProgressBarUtil
{
	public class ProgressBarOptions
	{
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

	public class ProgressBar : IDisposable
	{
		private readonly ProgressBarOptions _options;

		private readonly DateTime _startTime;

		private readonly char[] _spinnerChars;

		private int _current;

		private int _spinnerIndex;

		private bool _disposed;
		
		private int _total;

		public ProgressBar(int total, ProgressBarOptions? options = null)
		{
			_total = total;
			_options = options ?? new ProgressBarOptions();
			_startTime = DateTime.Now;
			_spinnerChars = ['|', '/', '─', '\\'];
		}

		public void Update(int current, string action)
		{
			if (!_disposed)
			{
				_current = current;
				_spinnerIndex = (_spinnerIndex + 1) % _spinnerChars.Length;
				Display(action);
			}
		}

		private void Display(string action)
		{
			using (Lock.EnterScope())
			{
				ClearCurrent();
				double num = (double)_current / (double)_total;
				int num2 = (int)(num * (double)_options.Width);
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(_options.Prefix);
				if (_options.ShowSpinner)
				{
					if (_current < _total)
					{
						stringBuilder.Append(GetAnsiColorCode(_options.SpinnerColor));
						stringBuilder.Append(_spinnerChars[_spinnerIndex]);
						stringBuilder.Append(GetAnsiColorCode(ConsoleColor.White));
						stringBuilder.Append(' ');
					}
					else if (_current >= _total)
					{
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
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder3 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
				handler.AppendLiteral("] ");
				handler.AppendFormatted(action);
				stringBuilder3.Append(ref handler);
				if (_options.ShowPercentage)
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder4 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(1, 1, stringBuilder2);
					handler.AppendLiteral(" ");
					handler.AppendFormatted(string.Format(_options.ProgressFormat, num));
					stringBuilder4.Append(ref handler);
				}
				TimeSpan value = DateTime.Now - _startTime;
				if (_options.ShowElapsedTime)
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder5 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(10, 1, stringBuilder2);
					handler.AppendLiteral(" Elapsed: ");
					handler.AppendFormatted(value, "mm\\:ss");
					stringBuilder5.Append(ref handler);
				}
				if (_options.ShowEta && _current > 0)
				{
					TimeSpan value2 = TimeSpan.FromMilliseconds(value.TotalMilliseconds / (double)_current * (double)(_total - _current));
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder6 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(6, 1, stringBuilder2);
					handler.AppendLiteral(" ETA: ");
					handler.AppendFormatted(value2, "mm\\:ss");
					stringBuilder6.Append(ref handler);
				}
				stringBuilder.Append(_options.Suffix);
				stringBuilder.Append("\u001b[0m");
				Console.Write($"\r{stringBuilder}");
				if (_current >= _total && _options.LastLineNewline)
				{
					Console.WriteLine();
				}
			}
		}

		public static void ClearCurrent()
		{
			Console.Write("\r" + new string(' ', Console.BufferWidth) + "\r");
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				if (_current < _total)
				{
					Update(_total, "Done");
				}
				_disposed = true;
			}
			GC.SuppressFinalize(this);
		}
	}

	public class ProgressReport
	{
		public int Percent { get; set; }

		public string Message { get; set; } = "";
	}

	private static readonly Lock Lock = new Lock();

	private static string GetAnsiColorCode(ConsoleColor color)
	{
		return color switch
		{
			ConsoleColor.Black => "\u001b[30m", 
			ConsoleColor.DarkRed => "\u001b[31m", 
			ConsoleColor.DarkGreen => "\u001b[32m", 
			ConsoleColor.DarkYellow => "\u001b[33m", 
			ConsoleColor.DarkBlue => "\u001b[34m", 
			ConsoleColor.DarkMagenta => "\u001b[35m", 
			ConsoleColor.DarkCyan => "\u001b[36m", 
			ConsoleColor.Gray => "\u001b[37m", 
			ConsoleColor.DarkGray => "\u001b[90m", 
			ConsoleColor.Red => "\u001b[91m", 
			ConsoleColor.Green => "\u001b[92m", 
			ConsoleColor.Yellow => "\u001b[93m", 
			ConsoleColor.Blue => "\u001b[94m", 
			ConsoleColor.Magenta => "\u001b[95m", 
			ConsoleColor.Cyan => "\u001b[96m", 
			ConsoleColor.White => "\u001b[97m", 
			_ => "\u001b[37m", 
		};
	}
}
