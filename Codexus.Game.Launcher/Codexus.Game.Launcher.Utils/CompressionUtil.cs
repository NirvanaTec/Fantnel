using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Codexus.Game.Launcher.Utils;

public static class CompressionUtil
{
	public static bool Extract7Z(string filePath, string outPath, Action<int> progressAction)
	{
		try
		{
			SevenZipArchive val = SevenZipArchive.Open(filePath, (ReaderOptions)null);
			try
			{
				IArchiveExtensions.ExtractToDirectory((IArchive)(object)val, outPath, (Action<double>)delegate(double dp)
				{
					progressAction((int)(dp * 100.0));
				}, default(CancellationToken));
				return true;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
			return ExtractZip(filePath, outPath, progressAction);
		}
	}

	private static bool ExtractZip(string filePath, string outPath, Action<int> progressAction)
	{
		try
		{
			ZipArchive val = ZipArchive.Open(filePath, (ReaderOptions)null);
			try
			{
				IArchiveExtensions.ExtractToDirectory((IArchive)(object)val, outPath, (Action<double>)delegate(double dp)
				{
					progressAction((int)(dp * 100.0));
				}, default(CancellationToken));
				return true;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
			return false;
		}
	}

	public static async Task Extract7ZAsync(string archivePath, string outputDir, Action<int>? progress = null)
	{
		await Task.Run(delegate
		{
			IArchive val = ArchiveFactory.Open(archivePath, (ReaderOptions)null);
			try
			{
				int num = val.Entries.Count();
				int num2 = 0;
				foreach (IArchiveEntry entry in val.Entries)
				{
					if (entry != null && !((IEntry)entry).IsDirectory && ((IEntry)entry).Key != null)
					{
						string path = Path.Combine(outputDir, ((IEntry)entry).Key);
						string directoryName = Path.GetDirectoryName(path);
						if (directoryName == null)
						{
							throw new ArgumentException("Invalid directory name");
						}
						if (!Directory.Exists(directoryName))
						{
							Directory.CreateDirectory(directoryName);
						}
						using Stream stream = entry.OpenEntryStream();
						using FileStream destination = File.Create(path);
						stream.CopyTo(destination);
					}
					num2++;
					progress?.Invoke((int)((double)num2 / (double)num * 100.0));
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		});
	}
}
