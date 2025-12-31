using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Codexus.Cipher.Entities;
using Codexus.Cipher.Entities.WPFLauncher.Minecraft;
using Codexus.Cipher.Entities.WPFLauncher.NetGame;
using Codexus.Cipher.Entities.WPFLauncher.NetGame.Mods;
using Codexus.Cipher.Entities.WPFLauncher.NetGame.Texture;
using Codexus.Cipher.Protocol;
using Codexus.Game.Launcher.Utils;
using Codexus.Game.Launcher.Utils.Progress;
using Serilog;

namespace Codexus.Game.Launcher.Services.Java;

public static class InstallerService
{
	public static async Task<bool> PrepareMinecraftClient(string userId, string userToken, WPFLauncher wpfLauncher, EnumGameVersion gameVersion)
	{
		string versionName = Enum.GetName(gameVersion);
		string md5Path = Path.Combine(PathUtil.GameBasePath, "GAME_BASE.MD5");
		string zipPath = Path.Combine(PathUtil.CachePath, "GameBase.7z");
		string versionMd5File = Path.Combine(PathUtil.GameBasePath, versionName + ".MD5");
		string versionZip = Path.Combine(PathUtil.CachePath, versionName + ".7z");
		string libMd5File = Path.Combine(PathUtil.GameBasePath, versionName + "_Lib.MD5");
		string libZip = Path.Combine(PathUtil.CachePath, versionName + "_Lib.7z");
		Entity<EntityCoreLibResponse> minecraftClientLibs = wpfLauncher.GetMinecraftClientLibs(userId, userToken);
		if (minecraftClientLibs.Code != 0)
		{
			throw new Exception("Failed to fetch base package: " + minecraftClientLibs.Message);
		}
		await ProcessPackage(minecraftClientLibs.Data.Url, zipPath, PathUtil.GameBasePath, md5Path, minecraftClientLibs.Data.Md5, "base package");
		Entity<EntityCoreLibResponse> versionResult = wpfLauncher.GetMinecraftClientLibs(userId, userToken, gameVersion);
		if (versionResult.Code != 0)
		{
			throw new Exception("Failed to fetch " + versionName + " package: " + versionResult.Message);
		}
		await ProcessPackage(versionResult.Data.Url, versionZip, PathUtil.GameBasePath, versionMd5File, versionResult.Data.Md5, versionName + " package");
		await ProcessPackage(versionResult.Data.CoreLibUrl, libZip, PathUtil.CachePath, libMd5File, versionResult.Data.CoreLibMd5, versionName + " libraries");
		InstallCoreLibs(Path.Combine(PathUtil.CachePath, versionName + "_libs"), gameVersion);
		return true;
	}

	private static async Task ProcessPackage(string url, string zipPath, string extractTo, string md5Path, string md5, string label)
	{
		bool flag = File.Exists(md5Path);
		if (flag)
		{
			flag = await File.ReadAllTextAsync(md5Path) == md5;
		}
		if (!flag)
		{
			SyncProgressBarUtil.ProgressBar progress = new SyncProgressBarUtil.ProgressBar(100);
			IProgress<SyncProgressBarUtil.ProgressReport> uiProgress = new SyncCallback<SyncProgressBarUtil.ProgressReport>(delegate(SyncProgressBarUtil.ProgressReport update)
			{
				progress.Update(update.Percent, update.Message);
			});
			await DownloadUtil.DownloadAsync(url, zipPath, delegate(uint p)
			{
				uiProgress.Report(new SyncProgressBarUtil.ProgressReport
				{
					Percent = (int)p,
					Message = "Downloading " + label
				});
			});
			CompressionUtil.Extract7Z(zipPath, extractTo, delegate(int p)
			{
				uiProgress.Report(new SyncProgressBarUtil.ProgressReport
				{
					Percent = p,
					Message = "Extracting " + label
				});
			});
			await File.WriteAllTextAsync(md5Path, md5);
			FileUtil.DeleteFileSafe(zipPath);
		}
	}

	private static void InstallCoreLibs(string libPath, EnumGameVersion gameVersion)
	{
		string gameVersionFromEnum = GameVersionUtil.GetGameVersionFromEnum(gameVersion);
		string text = "forge-" + gameVersionFromEnum + "-";
		string text2 = "launchwrapper-";
		string text3 = "MercuriusUpdater-";
		string text4 = gameVersionFromEnum + ".jar";
		string text5 = gameVersionFromEnum + ".json";
		if (!Directory.Exists(libPath))
		{
			return;
		}
		string[] files = Directory.GetFiles(libPath, "*", SearchOption.AllDirectories);
		foreach (string text6 in files)
		{
			string fileName = Path.GetFileName(text6);
			if (fileName.StartsWith(text))
			{
				text = Path.GetFileNameWithoutExtension(text6);
				string path = text.Replace("forge-", "");
				string text7 = Path.Combine(PathUtil.GameBasePath, ".minecraft", "libraries\\net\\minecraftforge\\forge", path);
				string text8 = Path.Combine(text7, text + ".jar");
				if (!Directory.Exists(text7))
				{
					Directory.CreateDirectory(text7);
				}
				else if (File.Exists(text8))
				{
					File.Delete(text8);
				}
				File.Copy(text6, text8, overwrite: true);
			}
			else if (fileName.StartsWith(text2))
			{
				text2 = Path.GetFileNameWithoutExtension(text6);
				string path2 = text2.Replace("launchwrapper-", "");
				string text9 = Path.Combine(PathUtil.GameBasePath, ".minecraft", "libraries\\net\\minecraft\\launchwrapper", path2);
				string text10 = Path.Combine(text9, text2 + ".jar");
				if (!Directory.Exists(text9))
				{
					Directory.CreateDirectory(text9);
				}
				else if (File.Exists(text10))
				{
					File.Delete(text10);
				}
				File.Copy(text6, text10, overwrite: true);
			}
			else if (fileName.StartsWith(text3))
			{
				text3 = Path.GetFileNameWithoutExtension(text6);
				string path3 = text3.Replace("MercuriusUpdater-", "");
				string text11 = Path.Combine(PathUtil.GameBasePath, ".minecraft", "libraries\\net\\minecraftforge\\MercuriusUpdater", path3);
				string text12 = Path.Combine(text11, text3 + ".jar");
				if (!Directory.Exists(text11))
				{
					Directory.CreateDirectory(text11);
				}
				else if (File.Exists(text12))
				{
					File.Delete(text12);
				}
				File.Copy(text6, text12, overwrite: true);
			}
			else if (fileName.Equals(text4))
			{
				string destFileName = Path.Combine(PathUtil.GameBasePath, ".minecraft", "versions", gameVersionFromEnum, text4);
				File.Copy(text6, destFileName, overwrite: true);
			}
			else if (fileName.Equals(text5))
			{
				string destFileName2 = Path.Combine(PathUtil.GameBasePath, ".minecraft", "versions", gameVersionFromEnum, text5);
				File.Copy(text6, destFileName2, overwrite: true);
			}
			else if (fileName.StartsWith("modlauncher-") && fileName.Contains("9.1.0"))
			{
				string destFileName3 = Path.Combine(new string[3]
				{
					PathUtil.GameBasePath,
					".minecraft",
					"libraries\\cpw\\mods\\modlauncher\\9.1.0\\modlauncher-9.1.0.jar"
				});
				File.Copy(text6, destFileName3, overwrite: true);
			}
			else if (fileName.StartsWith("modlauncher-") && fileName.Contains("10.0.9"))
			{
				string destFileName4 = Path.Combine(new string[3]
				{
					PathUtil.GameBasePath,
					".minecraft",
					"libraries\\cpw\\mods\\modlauncher\\10.0.9\\modlauncher-10.0.9.jar"
				});
				File.Copy(text6, destFileName4, overwrite: true);
			}
			else if (fileName.StartsWith("modlauncher-") && fileName.Contains("10.2.1"))
			{
				string destFileName5 = Path.Combine(new string[3]
				{
					PathUtil.GameBasePath,
					".minecraft",
					"libraries\\net\\minecraftforge\\modlauncher\\10.2.1\\modlauncher-10.2.1.jar"
				});
				File.Copy(text6, destFileName5, overwrite: true);
			}
		}
		FileUtil.DeleteDirectorySafe(libPath);
	}

	public static async Task<EntityModsList?> InstallGameMods(string userId, string userToken, EnumGameVersion gameVersion, WPFLauncher wpfLauncher, string gameId, bool isRental)
	{
		Entity<EntityQuerySearchByGameResponse> entity = await wpfLauncher.GetGameCoreModListAsync(userId, userToken, gameVersion, isRental);
		if (entity.Data?.IidList == null)
		{
			return null;
		}
		Entities<EntityComponentDownloadInfoResponse> entities = await wpfLauncher.GetGameCoreModDetailsListAsync(userId, userToken, entity.Data.IidList);
		EntityModsList modList = new EntityModsList();
		EntityComponentDownloadInfoResponse[] data = entities.Data;
		foreach (EntityComponentDownloadInfoResponse entityComponentDownloadInfoResponse in data)
		{
			foreach (EntityComponentDownloadInfoResponseSub subEntity in entityComponentDownloadInfoResponse.SubEntities)
			{
				modList.Mods.Add(new EntityModsInfo
				{
					ModPath = $"{entityComponentDownloadInfoResponse.ItemId}@{entityComponentDownloadInfoResponse.MTypeId}@0.jar",
					Id = $"{entityComponentDownloadInfoResponse.ItemId}@{entityComponentDownloadInfoResponse.MTypeId}@0.jar",
					Iid = entityComponentDownloadInfoResponse.ItemId,
					Md5 = subEntity.JarMd5.ToUpper(),
					Name = "",
					Version = ""
				});
			}
		}
		SyncProgressBarUtil.ProgressBar progress = new SyncProgressBarUtil.ProgressBar(100);
		IProgress<SyncProgressBarUtil.ProgressReport> uiProgress = new SyncCallback<SyncProgressBarUtil.ProgressReport>(delegate(SyncProgressBarUtil.ProgressReport update)
		{
			progress.Update(update.Percent, update.Message);
		});
		string corePath = Path.Combine(PathUtil.GameModsPath, gameId);
		if (Directory.Exists(corePath))
		{
			Directory.Delete(corePath, recursive: true);
		}
		int total = entities.Total;
		int idx = 0;
		EntityComponentDownloadInfoResponse[] data2 = entities.Data;
		foreach (EntityComponentDownloadInfoResponse entityComponentDownloadInfoResponse2 in data2)
		{
			int i = idx;
			idx = i + 1;
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(entityComponentDownloadInfoResponse2.SubEntities[0].ResName);
			string jar = Path.Combine(corePath, $"{fileNameWithoutExtension}@{entityComponentDownloadInfoResponse2.MTypeId}@{entityComponentDownloadInfoResponse2.EntityId}.jar");
			string archive = Path.Combine(corePath, entityComponentDownloadInfoResponse2.SubEntities[0].ResName);
			string extractDir = Path.Combine(corePath, fileNameWithoutExtension);
			if (!File.Exists(jar) || !FileUtil.ComputeMd5FromFile(jar).Equals(entityComponentDownloadInfoResponse2.SubEntities[0].JarMd5, StringComparison.OrdinalIgnoreCase))
			{
				await DownloadUtil.DownloadAsync(entityComponentDownloadInfoResponse2.SubEntities[0].ResUrl, archive, delegate(uint dp)
				{
					uiProgress.Report(new SyncProgressBarUtil.ProgressReport
					{
						Percent = (int)dp,
						Message = $"Downloading core mod {idx}/{total}"
					});
				});
				int idx2 = idx;
				CompressionUtil.Extract7Z(archive, extractDir, delegate(int p)
				{
					uiProgress.Report(new SyncProgressBarUtil.ProgressReport
					{
						Percent = p * 100 / total,
						Message = $"Extracting core mod {idx2}/{total}"
					});
				});
				string[] array = FileUtil.EnumerateFiles(extractDir, "jar");
				for (i = 0; i < array.Length; i++)
				{
					FileUtil.CopyFileSafe(array[i], jar);
				}
				FileUtil.DeleteDirectorySafe(extractDir);
				FileUtil.DeleteFileSafe(archive);
			}
		}
		uiProgress.Report(new SyncProgressBarUtil.ProgressReport
		{
			Percent = 100,
			Message = "Core mods ready"
		});
		string compDir = Path.Combine(PathUtil.CachePath, "Game", gameId);
		string compArchive = compDir + ".7z";
		FileUtil.CreateDirectorySafe(compDir);
		Entity<EntityComponentDownloadInfoResponse> netGameComponentDownloadList = wpfLauncher.GetNetGameComponentDownloadList(userId, userToken, gameId);
		if (netGameComponentDownloadList != null && netGameComponentDownloadList.Data != null && netGameComponentDownloadList.Code == 0)
		{
			EntityComponentDownloadInfoResponseSub comp = netGameComponentDownloadList.Data?.SubEntities[0];
			string extractDir = Path.Combine(compDir, gameId + ".MD5");
			string archive = Path.Combine(compDir, gameId + ".json");
			bool flag = !File.Exists(extractDir);
			if (!flag)
			{
				flag = await File.ReadAllTextAsync(extractDir) != comp?.ResMd5;
			}
			if (!flag && File.Exists(archive))
			{
				foreach (EntityModsInfo mod in JsonSerializer.Deserialize<EntityModsList>(await File.ReadAllTextAsync(archive)).Mods)
				{
					modList.Mods.Add(mod);
				}
				uiProgress.Report(new SyncProgressBarUtil.ProgressReport
				{
					Percent = 100,
					Message = "Game assets ready"
				});
				SyncProgressBarUtil.ProgressBar.ClearCurrent();
				return modList;
			}
			FileUtil.DeleteFileSafe(compArchive);
			await DownloadUtil.DownloadAsync(comp.ResUrl, compArchive, delegate(uint p)
			{
				uiProgress.Report(new SyncProgressBarUtil.ProgressReport
				{
					Percent = (int)p,
					Message = "Downloading game assets"
				});
			});
			FileUtil.DeleteDirectorySafe(compDir);
			CompressionUtil.Extract7Z(compArchive, compDir, delegate(int p)
			{
				uiProgress.Report(new SyncProgressBarUtil.ProgressReport
				{
					Percent = p,
					Message = "Extracting game assets"
				});
			});
			string[] array2 = FileUtil.EnumerateFiles(Path.Combine(compDir, ".minecraft", "mods"), "jar");
			EntityModsList serverModsList = new EntityModsList();
			string[] array3 = array2;
			foreach (string path in array3)
			{
				string jar = Path.GetFileName(path);
				string md = Convert.ToHexString(MD5.HashData(await File.ReadAllBytesAsync(path))).ToUpper();
				serverModsList.Mods.Add(new EntityModsInfo
				{
					Name = "",
					Version = "",
					ModPath = jar,
					Id = jar,
					Iid = jar.Split('@')[0],
					Md5 = md
				});
			}
			modList.Mods.AddRange(serverModsList.Mods);
			await File.WriteAllTextAsync(extractDir, comp.ResMd5);
			await File.WriteAllTextAsync(archive, JsonSerializer.Serialize(serverModsList));
			FileUtil.DeleteFileSafe(compArchive);
		}
		uiProgress.Report(new SyncProgressBarUtil.ProgressReport
		{
			Percent = 100,
			Message = "Game assets ready"
		});
		SyncProgressBarUtil.ProgressBar.ClearCurrent();
		return modList;
	}

	private static void InstallCustomMods(string mods)
	{
		FileUtil.EnumerateFiles(PathUtil.CustomModsPath, "jar").ToList().ForEach(delegate(string f)
		{
			FileUtil.CopyFileSafe(f, Path.Combine(mods, Path.GetFileName(f)));
		});
	}

	public static string PrepareGameRuntime(string userId, string gameId, string roleName, EnumGType gameType)
	{
		string path = HashUtil.GenerateGameRuntimeId(gameId, roleName);
		string text = Path.Combine(PathUtil.GamePath, "Runtime", path);
		string text2 = Path.Combine(text, ".minecraft");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		if (gameType == EnumGType.NetGame)
		{
			string text3 = Path.Combine(text2, "mods");
			FileUtil.DeleteDirectorySafe(text3);
			FileUtil.CreateDirectorySafe(text3);
			FileUtil.CopyDirectory(Path.Combine(PathUtil.CachePath, "Game", gameId, ".minecraft"), text2, includeRoot: false);
			InstallCustomMods(text3);
		}
		string text4 = Path.Combine(text2, "assets");
		string targetPath = Path.Combine(PathUtil.GameBasePath, ".minecraft", "assets");
		if (Directory.Exists(text4))
		{
			FileUtil.DeleteDirectorySafe(text4);
		}
		FileUtil.CreateSymbolicLinkSafe(text4, targetPath);
		return text;
	}

	public static void InstallCoreMods(string gameId, string targetModsPath)
	{
		string text = Path.Combine(PathUtil.GameModsPath, gameId);
		if (Directory.Exists(text))
		{
			FileUtil.CreateDirectorySafe(targetModsPath);
			string[] array = FileUtil.EnumerateFiles(text);
			foreach (string text2 in array)
			{
				string text3 = Path.Combine(targetModsPath, Path.GetRelativePath(text, text2));
				FileUtil.CreateDirectorySafe(Path.GetDirectoryName(text3));
				FileUtil.CopyFileSafe(text2, text3);
			}
		}
	}

	public static void InstallNativeDll(EnumGameVersion gameVersion)
	{
		try
		{
			string text = Path.Combine(PathUtil.ResourcePath, "api-ms-win-crt-utility-l1-1-1.dll");
			string text2 = Path.Combine(
				PathUtil.GameBasePath,
				".minecraft",
				"versions",
				GameVersionUtil.GetGameVersionFromEnum(gameVersion),
				"natives",
				"runtime"
			);
			if (!Directory.Exists(text2))
			{
				FileUtil.CreateDirectorySafe(text2);
			}
			if (!File.Exists(text))
			{
				throw new Exception("Native dll not found: " + text);
			}
			string destPath = Path.Combine(text2, "api-ms-win-crt-utility-l1-1-1.dll");
			FileUtil.CopyFileSafe(text, destPath);
		}
		catch (Exception ex)
		{
			Log.Error("Failed to install native dll:" + ex);
		}
	}
}
