using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Serilog;

namespace Codexus.Game.Launcher.Utils;

public partial class MemoryOptimizer : IDisposable
{
	private const uint ProcessQueryInformation = 1024u;

	private const uint ProcessSetQuota = 256u;

	private static MemoryOptimizer? _instance;

	private static readonly Lock Lock = new Lock();

	private Timer? _optimizationTimer;

	private readonly HashSet<int> _processedIds = new HashSet<int>();

	private readonly Lock _lockObject = new Lock();

	private bool _disposed;

	
	[DllImport("kernel32.dll", ExactSpelling = true)]
	private static extern nint OpenProcess(uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwProcessId);

	[DllImport("kernel32.dll", ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CloseHandle(nint hObject);

	[DllImport("kernel32.dll", ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetProcessWorkingSetSize(nint hProcess, nint dwMinimumWorkingSetSize, nint dwMaximumWorkingSetSize);

	[DllImport("psapi.dll", ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool EmptyWorkingSet(nint hProcess);
	
	private MemoryOptimizer()
	{
		_optimizationTimer = new Timer(OptimizeCallback, null, TimeSpan.Zero, TimeSpan.FromMinutes(7L));
	}

	public static MemoryOptimizer GetInstance()
	{
		using (Lock.EnterScope())
		{
			return _instance ?? (_instance = new MemoryOptimizer());
		}
	}

	private void OptimizeCallback(object? state)
	{
		if (_disposed || !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			Dispose();
			return;
		}
		try
		{
			List<Process> minecraftProcesses = GetMinecraftProcesses();
			if (minecraftProcesses.Count == 0)
			{
				Log.Information("[Memory Optimizer] No Minecraft processes found, stopping optimizer");
				Dispose();
				return;
			}
			foreach (Process item in minecraftProcesses)
			{
				OptimizeProcess(item);
			}
			CleanupExitedProcesses();
		}
		catch (Exception ex)
		{
			Log.Error(ex, "[Memory Optimizer] Error in optimization callback");
		}
	}

	private static List<Process> GetMinecraftProcesses()
	{
		List<Process> list = new List<Process>();
		try
		{
			Process[] processesByName = Process.GetProcessesByName("javaw");
			list.AddRange(processesByName.Where(IsMinecraftProcess));
		}
		catch (Exception ex)
		{
			Log.Error(ex, "[Memory Optimizer] Failed to get Minecraft processes");
		}
		return list;
	}

	private static bool IsMinecraftProcess(Process process)
	{
		try
		{
			string commandLine = GetCommandLine(process);
			if (string.IsNullOrEmpty(commandLine))
			{
				return false;
			}
			return new string[10] { "minecraft", "net.minecraft", "launchwrapper", "forge", "fabric", "quilt", "optifine", ".minecraft", "versions", "libraries" }.Any((string keyword) => commandLine.Contains(keyword, StringComparison.OrdinalIgnoreCase));
		}
		catch (Exception ex)
		{
			Log.Warning<int>(ex, "[Memory Optimizer] Failed to check if process is Minecraft: {ProcessId}", process.Id);
			return false;
		}
	}

	private static string? GetCommandLine(Process process)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		try
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return null;
			}
			ManagementObjectSearcher val = new ManagementObjectSearcher($"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}");
			try
			{
				ManagementObjectCollection val2 = val.Get();
				try
				{
					ManagementObjectCollection.ManagementObjectEnumerator enumerator = val2.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							ManagementBaseObject current = enumerator.Current;
							ManagementObject val3 = (ManagementObject)(object)((current is ManagementObject) ? current : null);
							if (val3 != null)
							{
								return ((ManagementBaseObject)val3)["CommandLine"]?.ToString() ?? "";
							}
						}
					}
					finally
					{
						((IDisposable)enumerator)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
			try
			{
				return process.StartInfo.Arguments;
			}
			catch
			{
				Log.Error("[Memory Optimizer] Failed to get CommandLine arguments");
				return null;
			}
		}
		return "";
	}

	private void OptimizeProcess(Process process)
	{
		if (_disposed)
		{
			return;
		}
		using (_lockObject.EnterScope())
		{
			try
			{
				if (process.HasExited)
				{
					return;
				}
				process.Refresh();
				long workingSet = process.WorkingSet64;
				long num = workingSet / 1048576;
				nint num2 = OpenProcess(1280u, bInheritHandle: false, (uint)process.Id);
				if (num2 == IntPtr.Zero)
				{
					return;
				}
				try
				{
					if (EmptyWorkingSet(num2))
					{
						long num3 = Math.Max(52428800L, workingSet / 4);
						long value = Math.Max(num3 * 2, workingSet);
						SetProcessWorkingSetSize(num2, new IntPtr(num3), new IntPtr(value));
						Thread.Sleep(500);
						process.Refresh();
						long num4 = process.WorkingSet64 / 1048576;
						Log.Information<int, long, long>("[Memory Optimizer] Process ID: {ProcessId} - Memory Before: {BeforeMemory} MB, After: {AfterMemory} MB", process.Id, num, num4);
						_processedIds.Add(process.Id);
					}
				}
				finally
				{
					CloseHandle(num2);
				}
			}
			catch (Exception ex)
			{
				Log.Error<int>(ex, "[Memory Optimizer] Failed to optimize process {ProcessId}", process.Id);
			}
		}
	}

	private void CleanupExitedProcesses()
	{
		if (_disposed)
		{
			return;
		}
		using (_lockObject.EnterScope())
		{
			List<int> list = new List<int>();
			foreach (int processedId in _processedIds)
			{
				try
				{
					Process processById = Process.GetProcessById(processedId);
					if (processById.HasExited)
					{
						list.Add(processedId);
						processById.Dispose();
					}
				}
				catch (ArgumentException)
				{
					list.Add(processedId);
				}
				catch (Exception ex2)
				{
					Log.Warning<int>(ex2, "[Memory Optimizer] Error checking process {ProcessId}", processedId);
					list.Add(processedId);
				}
			}
			foreach (int item in list)
			{
				_processedIds.Remove(item);
			}
			if (list.Count > 0)
			{
				Log.Information<int>("[Memory Optimizer] Cleaned up {Count} exited processes", list.Count);
			}
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		using (Lock.EnterScope())
		{
			if (!_disposed)
			{
				_disposed = true;
				_optimizationTimer?.Dispose();
				_optimizationTimer = null;
				_instance = null;
				Log.Information("[Memory Optimizer] Disposed");
			}
		}
	}
}
