using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Codexus.Game.Launcher.Utils;

public static class FileUtil
{
	public static string[] EnumerateFiles(string path, string? fileType = null)
	{
		if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
		{
			return Array.Empty<string>();
		}
		string searchPattern = (string.IsNullOrWhiteSpace(fileType) ? "*" : ("*." + fileType.TrimStart('.').ToLowerInvariant()));
		return Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories).ToArray();
	}

	public static string ComputeMd5FromFile(string path)
	{
		if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
		{
			return string.Empty;
		}
		try
		{
			using FileStream inputStream = File.OpenRead(path);
			using MD5 mD = MD5.Create();
			return Convert.ToHexString(mD.ComputeHash(inputStream)).ToLowerInvariant();
		}
		catch (IOException)
		{
			return string.Empty;
		}
		catch (UnauthorizedAccessException)
		{
			return string.Empty;
		}
	}

	public static bool CreateSymbolicLinkSafe(string linkPath, string targetPath)
	{
		if (string.IsNullOrWhiteSpace(linkPath) || string.IsNullOrWhiteSpace(targetPath))
		{
			return false;
		}
		try
		{
			bool flag = Directory.Exists(targetPath);
			if (File.Exists(linkPath) || Directory.Exists(linkPath))
			{
				FileSystemInfo fileSystemInfo = (flag ? ((FileSystemInfo)new DirectoryInfo(linkPath)) : ((FileSystemInfo)new FileInfo(linkPath)));
				if (fileSystemInfo.LinkTarget != null && Path.GetFullPath(fileSystemInfo.LinkTarget) == Path.GetFullPath(targetPath))
				{
					return true;
				}
				fileSystemInfo.Delete();
			}
			if (flag)
			{
				Directory.CreateSymbolicLink(linkPath, targetPath);
			}
			else
			{
				File.CreateSymbolicLink(linkPath, targetPath);
			}
			return true;
		}
		catch (IOException)
		{
			return false;
		}
		catch (UnauthorizedAccessException)
		{
			return false;
		}
		catch (PlatformNotSupportedException)
		{
			return false;
		}
	}

	public static void CleanDirectorySafe(string path)
	{
		if (!string.IsNullOrWhiteSpace(path))
		{
			DeleteDirectorySafe(path);
			CreateDirectorySafe(path);
		}
	}

	public static bool CreateDirectorySafe(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return false;
		}
		try
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			return true;
		}
		catch (IOException)
		{
			return false;
		}
		catch (UnauthorizedAccessException)
		{
			return false;
		}
	}

	public static bool DeleteDirectorySafe(string path)
	{
		if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
		{
			return true;
		}
		try
		{
			Directory.Delete(path, recursive: true);
			return true;
		}
		catch (IOException)
		{
			return false;
		}
		catch (UnauthorizedAccessException)
		{
			return false;
		}
	}

	public static bool CopyFileSafe(string sourcePath, string destPath)
	{
		if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(destPath))
		{
			return false;
		}
		try
		{
			string directoryName = Path.GetDirectoryName(destPath);
			if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			File.Copy(sourcePath, destPath, overwrite: true);
			return true;
		}
		catch (IOException)
		{
			return false;
		}
		catch (UnauthorizedAccessException)
		{
			return false;
		}
	}

	public static void CopyDirectory(string sourceDir, string targetDir, bool includeRoot = true, bool deleteSource = false)
	{
		if (string.IsNullOrWhiteSpace(sourceDir) || !Directory.Exists(sourceDir))
		{
			return;
		}
		string name = new DirectoryInfo(sourceDir).Name;
		string text = (includeRoot ? Path.Combine(targetDir, name) : targetDir);
		Directory.CreateDirectory(text);
		foreach (string item in Directory.EnumerateFileSystemEntries(sourceDir))
		{
			string fileName = Path.GetFileName(item);
			string destFileName = Path.Combine(text, fileName);
			if (Directory.Exists(item))
			{
				CopyDirectory(item, text, includeRoot: true, deleteSource);
				if (deleteSource)
				{
					Directory.Delete(item, recursive: true);
				}
			}
			else
			{
				File.Copy(item, destFileName, overwrite: true);
				if (deleteSource)
				{
					File.Delete(item);
				}
			}
		}
	}

	public static bool DeleteFileSafe(string path)
	{
		try
		{
			if (!File.Exists(path))
			{
				return true;
			}
			File.Delete(path);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static bool IsFileReadable(string filePath)
	{
		try
		{
			using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			return fileStream.Length > 0;
		}
		catch
		{
			return false;
		}
	}

	public static async Task WriteFileSafelyAsync(string filePath, byte[] buffer)
	{
		string tempFile = filePath + ".tmp";
		try
		{
			await File.WriteAllBytesAsync(tempFile, buffer);
			if (File.Exists(tempFile) && new FileInfo(tempFile).Length > 0)
			{
				if (File.Exists(filePath))
				{
					File.Delete(filePath);
				}
				File.Move(tempFile, filePath);
			}
		}
		catch
		{
			if (File.Exists(tempFile))
			{
				File.Delete(tempFile);
			}
			throw;
		}
	}
}
